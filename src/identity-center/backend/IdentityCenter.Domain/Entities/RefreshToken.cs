using IdentityCenter.Domain.Common;

namespace IdentityCenter.Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
    public Guid UsuarioId { get; private set; }
    public Usuario Usuario { get; private set; } = null!;

    public string Token { get; private set; } = null!;
    public string DeviceId { get; private set; } = null!;
    public string SessionFingerprint { get; private set; } = null!;
    public string UserAgent { get; private set; } = null!;
    public DateTime ExpiraEn { get; private set; }
    public DateTime CreadoEn { get; private set; } = DateTime.UtcNow;
    public DateTime? UltimoUsoEn { get; private set; }
    public bool Revocado { get; private set; }
    public DateTime? RevocadoEn { get; private set; }

    private RefreshToken() { }

    public RefreshToken(Guid usuarioId, string token, DateTime expiraEn,
                        string deviceId, string sessionFingerprint, string userAgent)
    {
        UsuarioId = usuarioId;
        Token = token;
        ExpiraEn = expiraEn;
        DeviceId = deviceId;
        SessionFingerprint = sessionFingerprint;
        UserAgent = userAgent;
    }

    public bool EstaActivo => !Revocado && ExpiraEn > DateTime.UtcNow;

    public bool PerteneceASesion(string sessionFingerprint)
        => SessionFingerprint == sessionFingerprint;

    public void RegistrarUso()
    {
        UltimoUsoEn = DateTime.UtcNow;
    }

    public void Revocar()
    {
        Revocado = true;
        RevocadoEn = DateTime.UtcNow;
    }
}
