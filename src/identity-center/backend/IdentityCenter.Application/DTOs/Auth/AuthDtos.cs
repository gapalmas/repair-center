namespace IdentityCenter.Application.DTOs.Auth;

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record RefreshTokenRequest(
    string RefreshToken);

public sealed record SessionContext(
    string DeviceId,
    string SessionFingerprint,
    string UserAgent);

public sealed record AuthTokensResult(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    DateTime RefreshTokenExpiresAt,
    UsuarioInfo Usuario);

public sealed record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt,
    UsuarioInfo Usuario,
    string CsrfToken);

public sealed record UsuarioInfo(
    Guid Id,
    Guid TenantId,
    Guid RolId,
    string Nombre,
    string Apellido,
    string Email,
    string Rol,
    bool DebeCambiarPassword);
