using IdentityCenter.Domain.Common;
using IdentityCenter.Domain.Enums;

namespace IdentityCenter.Domain.Entities;

public sealed class Usuario : AuditableEntity
{
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;

    public string Nombre { get; private set; } = null!;
    public string Apellido { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public bool DebeCambiarPassword { get; private set; }
    public Guid RolId { get; private set; }
    public Rol Rol { get; private set; } = null!;
    public EstadoUsuario Estado { get; private set; } = EstadoUsuario.Activo;

    private readonly List<RefreshToken> _refreshTokens = [];
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    private Usuario() { }

    public Usuario(Guid tenantId, string nombre, string apellido,
                   string email, string passwordHash, Guid rolId)
    {
        TenantId = tenantId;
        Nombre = nombre;
        Apellido = apellido;
        Email = email;
        PasswordHash = passwordHash;
        RolId = rolId;
    }

    public void Actualizar(string nombre, string apellido, Guid rolId)
    {
        Nombre = nombre;
        Apellido = apellido;
        RolId = rolId;
        ModificadoEn = DateTime.UtcNow;
    }

    public void CambiarPassword(string nuevoHash)
    {
        PasswordHash = nuevoHash;
        DebeCambiarPassword = false;
        ModificadoEn = DateTime.UtcNow;
    }

    public void MarcarCambioPasswordRequerido()
    {
        DebeCambiarPassword = true;
        ModificadoEn = DateTime.UtcNow;
    }

    public void Bloquear()
    {
        Estado = EstadoUsuario.Bloqueado;
        ModificadoEn = DateTime.UtcNow;
    }

    public void Activar()
    {
        Estado = EstadoUsuario.Activo;
        ModificadoEn = DateTime.UtcNow;
    }

    public void Desactivar()
    {
        Estado = EstadoUsuario.Inactivo;
        ModificadoEn = DateTime.UtcNow;
    }

    public void AgregarRefreshToken(RefreshToken token)
    {
        _refreshTokens.Add(token);
    }

    public void RevocarRefreshToken(string tokenValue)
    {
        var token = _refreshTokens.FirstOrDefault(t => t.Token == tokenValue && !t.Revocado);
        token?.Revocar();
    }

    public void RevocarTodosRefreshTokens()
    {
        foreach (var token in _refreshTokens.Where(t => !t.Revocado))
            token.Revocar();
    }

    public void RevocarRefreshTokensPorSesion(string sessionFingerprint)
    {
        foreach (var token in _refreshTokens.Where(t => !t.Revocado && t.PerteneceASesion(sessionFingerprint)))
            token.Revocar();
    }
}
