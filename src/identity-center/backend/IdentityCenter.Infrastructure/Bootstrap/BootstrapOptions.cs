namespace IdentityCenter.Infrastructure.Bootstrap;

public sealed class BootstrapOptions
{
    public const string SectionName = "Bootstrap";

    public bool Enabled { get; init; } = true;
    public bool ApplyMigrationsOnStartup { get; init; } = true;
    public bool EnsureBaseRoles { get; init; } = true;
    public bool EnsureSystemTenant { get; init; } = true;
    public bool EnsureInitialSuperAdmin { get; init; } = true;
    public bool AllowPasswordResetIfAdminExists { get; init; }
    public bool ForcePasswordChangeOnBootstrapAdmin { get; init; } = true;

    public string SystemTenantName { get; init; } = "Sistema Interno IdentityCenter";
    public string SystemTenantEmail { get; init; } = "identity@repairmodel.local";
    public string? SystemTenantDescription { get; init; } = "Tenant tecnico reservado para la administracion inicial del sistema.";

    public string AdminEmail { get; init; } = "admin@repairmodel.local";
    public string? AdminPassword { get; init; }
    public string AdminFirstName { get; init; } = "Administrador";
    public string AdminLastName { get; init; } = "Inicial";
}