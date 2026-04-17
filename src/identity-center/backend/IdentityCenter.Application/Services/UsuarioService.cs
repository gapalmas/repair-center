using IdentityCenter.Application.DTOs.Usuarios;
using IdentityCenter.Application.Interfaces;
using IdentityCenter.Domain.Entities;

namespace IdentityCenter.Application.Services;

public sealed class UsuarioService
{
    private readonly IUsuarioRepository _repo;
    private readonly ITenantRepository _tenantRepo;
    private readonly IRolRepository _rolRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IUnitOfWork _uow;

    public UsuarioService(IUsuarioRepository repo, ITenantRepository tenantRepo,
                          IRolRepository rolRepo,
                          IPasswordHasher hasher, IUnitOfWork uow)
    {
        _repo = repo;
        _tenantRepo = tenantRepo;
        _rolRepo = rolRepo;
        _hasher = hasher;
        _uow = uow;
    }

    public async Task<UsuarioDto> CrearAsync(CrearUsuarioRequest request, CancellationToken ct)
    {
        _ = await _tenantRepo.GetByIdAsync(request.TenantId, ct)
            ?? throw new KeyNotFoundException($"Tenant {request.TenantId} no encontrado.");

        var rol = await _rolRepo.GetByIdAsync(request.RolId, ct)
            ?? throw new KeyNotFoundException($"Rol {request.RolId} no encontrado.");

        if (!rol.Activo)
            throw new InvalidOperationException("No se puede asignar un rol inactivo.");

        var existente = await _repo.GetByEmailAsync(request.Email, ct);
        if (existente is not null)
            throw new InvalidOperationException($"Ya existe un usuario con email {request.Email}.");

        var hash = _hasher.Hash(request.Password);
        var usuario = new Usuario(request.TenantId, request.Nombre, request.Apellido,
                                  request.Email, hash, request.RolId);

        if (DebeForzarCambioPasswordInicial(rol.Nombre, request.Email))
        {
            usuario.MarcarCambioPasswordRequerido();
        }

        await _repo.AddAsync(usuario, ct);
        await _uow.SaveChangesAsync(ct);

        return usuario.ToDto();
    }

    public async Task<UsuarioDto> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var usuario = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

        return usuario.ToDto();
    }

    public async Task<List<UsuarioDto>> GetByTenantAsync(Guid tenantId, CancellationToken ct)
    {
        var usuarios = await _repo.GetByTenantIdAsync(tenantId, ct);
        return usuarios.Select(u => u.ToDto()).ToList();
    }

    public async Task<UsuarioDto> ActualizarAsync(Guid id, ActualizarUsuarioRequest request, CancellationToken ct)
    {
        var usuario = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

        var rol = await _rolRepo.GetByIdAsync(request.RolId, ct)
            ?? throw new KeyNotFoundException($"Rol {request.RolId} no encontrado.");

        if (!rol.Activo)
            throw new InvalidOperationException("No se puede asignar un rol inactivo.");

        usuario.Actualizar(request.Nombre, request.Apellido, request.RolId);
        _repo.Update(usuario);
        await _uow.SaveChangesAsync(ct);

        return usuario.ToDto();
    }

    public async Task CambiarPasswordAsync(Guid id, CambiarPasswordRequest request, CancellationToken ct)
    {
        var usuario = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

        if (!_hasher.Verify(request.PasswordActual, usuario.PasswordHash))
            throw new UnauthorizedAccessException("La contraseña actual es incorrecta.");

        var nuevoHash = _hasher.Hash(request.NuevoPassword);
        usuario.CambiarPassword(nuevoHash);
        _repo.Update(usuario);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task BloquearAsync(Guid id, CancellationToken ct)
    {
        var usuario = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

        usuario.Bloquear();
        _repo.Update(usuario);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task ActivarAsync(Guid id, CancellationToken ct)
    {
        var usuario = await _repo.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Usuario {id} no encontrado.");

        usuario.Activar();
        _repo.Update(usuario);
        await _uow.SaveChangesAsync(ct);
    }

    private static bool DebeForzarCambioPasswordInicial(string rolNombre, string email)
    {
        return rolNombre.Contains("admin", StringComparison.OrdinalIgnoreCase)
            || email.Equals("admin@repairmodel.local", StringComparison.OrdinalIgnoreCase);
    }
}

internal static class UsuarioMappingExtensions
{
    public static UsuarioDto ToDto(this Usuario usuario) =>
        new(usuario.Id, usuario.TenantId, usuario.RolId, usuario.Rol.Nombre, usuario.Rol.Activo,
            usuario.Nombre, usuario.Apellido, usuario.Email, usuario.Estado, usuario.CreadoEn);
}
