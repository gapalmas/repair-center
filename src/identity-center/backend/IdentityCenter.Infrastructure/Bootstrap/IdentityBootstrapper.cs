using IdentityCenter.Application.Interfaces;
using IdentityCenter.Domain.Entities;
using IdentityCenter.Domain.Enums;
using IdentityCenter.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityCenter.Infrastructure.Bootstrap;

public sealed class IdentityBootstrapper
{
    private static readonly (string Name, string Description)[] BaseRoles =
    [
        ("SuperAdmin", "Acceso total al modulo de identidad."),
        ("Admin", "Administracion operativa del modulo de identidad."),
        ("Tecnico", "Usuario tecnico que interactua con repair-center."),
        ("Recepcionista", "Recepcion y seguimiento inicial de equipos."),
        ("Almacenista", "Control de inventario y resguardo."),
        ("SoloLectura", "Consulta sin capacidad de modificacion.")
    ];

    private readonly IdentityDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly BootstrapOptions _options;
    private readonly ILogger<IdentityBootstrapper> _logger;

    public IdentityBootstrapper(
        IdentityDbContext db,
        IPasswordHasher hasher,
        IOptions<BootstrapOptions> options,
        ILogger<IdentityBootstrapper> logger)
    {
        _db = db;
        _hasher = hasher;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IdentityBootstrapResult> RunAsync(CancellationToken ct)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Bootstrap deshabilitado por configuracion.");
            return new IdentityBootstrapResult(false, false, false, false, false);
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        await _db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(641204062);", ct);

        var result = new IdentityBootstrapResult(true, false, false, false, false);

        if (_options.EnsureBaseRoles)
        {
            result = result with { RolesEnsured = await EnsureBaseRolesAsync(ct) };
        }

        Tenant? systemTenant = null;
        if (_options.EnsureSystemTenant)
        {
            var ensuredTenant = await EnsureSystemTenantAsync(ct);
            systemTenant = ensuredTenant.Tenant;
            result = result with { SystemTenantEnsured = ensuredTenant.CreatedOrUpdated };
        }

        if (_options.EnsureInitialSuperAdmin)
        {
            var ensuredAdmin = await EnsureInitialSuperAdminAsync(systemTenant, ct);
            result = result with
            {
                SuperAdminCreated = ensuredAdmin.Created,
                ExistingSuperAdminDetected = ensuredAdmin.ExistingSuperAdminDetected,
                AdminPasswordReset = ensuredAdmin.PasswordReset
            };
        }

        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        _logger.LogInformation(
            "Bootstrap completado. RolesEnsured={RolesEnsured}, SystemTenantEnsured={SystemTenantEnsured}, SuperAdminCreated={SuperAdminCreated}, ExistingSuperAdminDetected={ExistingSuperAdminDetected}, AdminPasswordReset={AdminPasswordReset}",
            result.RolesEnsured,
            result.SystemTenantEnsured,
            result.SuperAdminCreated,
            result.ExistingSuperAdminDetected,
            result.AdminPasswordReset);

        return result;
    }

    private async Task<bool> EnsureBaseRolesAsync(CancellationToken ct)
    {
        var createdOrUpdated = false;
        var existingRoles = await _db.Roles.ToListAsync(ct);

        foreach (var definition in BaseRoles)
        {
            var role = existingRoles.FirstOrDefault(static role => false);
            role = existingRoles.FirstOrDefault(role => role.Nombre.Equals(definition.Name, StringComparison.OrdinalIgnoreCase));

            if (role is null)
            {
                await _db.Roles.AddAsync(new Rol(definition.Name, definition.Description), ct);
                createdOrUpdated = true;
                continue;
            }

            if (!role.Activo || !string.Equals(role.Descripcion, definition.Description, StringComparison.Ordinal))
            {
                role.Actualizar(definition.Name, definition.Description);
                if (!role.Activo)
                {
                    role.Activar();
                }

                createdOrUpdated = true;
            }
        }

        return createdOrUpdated;
    }

    private async Task<(Tenant Tenant, bool CreatedOrUpdated)> EnsureSystemTenantAsync(CancellationToken ct)
    {
        ValidateSystemTenantOptions();

        var tenant = await _db.Tenants
            .FirstOrDefaultAsync(t =>
                t.EmailContacto == _options.SystemTenantEmail ||
                t.Nombre == _options.SystemTenantName,
                ct);

        if (tenant is null)
        {
            tenant = new Tenant(
                _options.SystemTenantName.Trim(),
                _options.SystemTenantEmail.Trim(),
                null,
                _options.SystemTenantDescription,
                null);

            await _db.Tenants.AddAsync(tenant, ct);
            return (tenant, true);
        }

        var changed =
            tenant.Nombre != _options.SystemTenantName.Trim() ||
            tenant.EmailContacto != _options.SystemTenantEmail.Trim() ||
            tenant.Rfc is not null ||
            tenant.Telefono is not null ||
            tenant.Direccion != _options.SystemTenantDescription ||
            tenant.Estado != EstadoTenant.Activo;

        if (changed)
        {
            tenant.Actualizar(
                _options.SystemTenantName.Trim(),
                _options.SystemTenantEmail.Trim(),
                null,
                _options.SystemTenantDescription,
                null);

            if (tenant.Estado != EstadoTenant.Activo)
            {
                tenant.Activar();
            }
        }

        return (tenant, changed);
    }

    private async Task<(bool Created, bool ExistingSuperAdminDetected, bool PasswordReset)> EnsureInitialSuperAdminAsync(Tenant? systemTenant, CancellationToken ct)
    {
        ValidateAdminOptions();

        systemTenant ??= await _db.Tenants.FirstOrDefaultAsync(
            tenant => tenant.EmailContacto == _options.SystemTenantEmail || tenant.Nombre == _options.SystemTenantName,
            ct)
            ?? throw new InvalidOperationException("No existe tenant interno para asociar el bootstrap del administrador.");

        var superAdminRole = await _db.Roles.FirstOrDefaultAsync(role => role.Nombre == "SuperAdmin", ct)
            ?? throw new InvalidOperationException("No existe el rol SuperAdmin requerido para el bootstrap.");

        var admin = await _db.Usuarios
            .Include(user => user.Rol)
            .FirstOrDefaultAsync(user => user.Email == _options.AdminEmail.Trim(), ct);

        var anyActiveSuperAdmin = await _db.Usuarios
            .Include(user => user.Rol)
            .AnyAsync(user => user.Rol.Nombre == "SuperAdmin" && user.Estado == EstadoUsuario.Activo, ct);

        if (admin is null)
        {
            if (anyActiveSuperAdmin)
            {
                _logger.LogInformation(
                    "Ya existe un SuperAdmin activo. No se crea el admin configurado {AdminEmail} automaticamente.",
                    _options.AdminEmail);

                return (false, true, false);
            }

            if (string.IsNullOrWhiteSpace(_options.AdminPassword))
            {
                throw new InvalidOperationException(
                    "Bootstrap requiere Bootstrap:AdminPassword para crear el primer SuperAdmin cuando no existe ninguno.");
            }

            var adminUser = new Usuario(
                systemTenant.Id,
                _options.AdminFirstName.Trim(),
                _options.AdminLastName.Trim(),
                _options.AdminEmail.Trim(),
                _hasher.Hash(_options.AdminPassword),
                superAdminRole.Id);

            if (_options.ForcePasswordChangeOnBootstrapAdmin)
            {
                adminUser.MarcarCambioPasswordRequerido();
            }

            await _db.Usuarios.AddAsync(adminUser, ct);
            return (true, false, false);
        }

        var changed = false;
        if (admin.TenantId != systemTenant.Id ||
            admin.RolId != superAdminRole.Id ||
            admin.Nombre != _options.AdminFirstName.Trim() ||
            admin.Apellido != _options.AdminLastName.Trim())
        {
            admin.Actualizar(_options.AdminFirstName.Trim(), _options.AdminLastName.Trim(), superAdminRole.Id);
            changed = true;
        }

        if (admin.Estado != EstadoUsuario.Activo)
        {
            admin.Activar();
            changed = true;
        }

        var passwordReset = false;
        if (_options.AllowPasswordResetIfAdminExists && !string.IsNullOrWhiteSpace(_options.AdminPassword))
        {
            admin.CambiarPassword(_hasher.Hash(_options.AdminPassword));

            if (_options.ForcePasswordChangeOnBootstrapAdmin)
            {
                admin.MarcarCambioPasswordRequerido();
            }

            passwordReset = true;
            changed = true;
        }

        return (false, true, passwordReset && changed);
    }

    private void ValidateSystemTenantOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.SystemTenantName))
            throw new InvalidOperationException("Bootstrap:SystemTenantName es requerido.");

        if (string.IsNullOrWhiteSpace(_options.SystemTenantEmail))
            throw new InvalidOperationException("Bootstrap:SystemTenantEmail es requerido.");
    }

    private void ValidateAdminOptions()
    {
        if (string.IsNullOrWhiteSpace(_options.AdminEmail))
            throw new InvalidOperationException("Bootstrap:AdminEmail es requerido.");

        if (string.IsNullOrWhiteSpace(_options.AdminFirstName))
            throw new InvalidOperationException("Bootstrap:AdminFirstName es requerido.");

        if (string.IsNullOrWhiteSpace(_options.AdminLastName))
            throw new InvalidOperationException("Bootstrap:AdminLastName es requerido.");
    }
}

public sealed record IdentityBootstrapResult(
    bool Enabled,
    bool RolesEnsured,
    bool SystemTenantEnsured,
    bool SuperAdminCreated,
    bool ExistingSuperAdminDetected,
    bool AdminPasswordReset = false);