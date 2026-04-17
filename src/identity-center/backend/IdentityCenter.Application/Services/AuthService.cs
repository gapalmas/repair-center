using IdentityCenter.Application.DTOs.Auth;
using IdentityCenter.Application.Interfaces;
using IdentityCenter.Domain.Entities;
using IdentityCenter.Domain.Enums;

namespace IdentityCenter.Application.Services;

public sealed class AuthService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IJwtTokenGenerator _jwt;
    private readonly IUnitOfWork _uow;
    private readonly int _refreshTokenDays;

    public AuthService(IUsuarioRepository usuarioRepo, IPasswordHasher hasher,
                       IJwtTokenGenerator jwt, IUnitOfWork uow, int refreshTokenDays = 7)
    {
        _usuarioRepo = usuarioRepo;
        _hasher = hasher;
        _jwt = jwt;
        _uow = uow;
        _refreshTokenDays = refreshTokenDays;
    }

    public async Task<AuthTokensResult> LoginAsync(LoginRequest request, SessionContext session, CancellationToken ct)
    {
        var usuario = await _usuarioRepo.GetByEmailAsync(request.Email, ct)
            ?? throw new UnauthorizedAccessException("Credenciales inválidas.");

        if (usuario.Estado != EstadoUsuario.Activo)
            throw new UnauthorizedAccessException("El usuario no está activo.");

        if (!usuario.Rol.Activo)
            throw new UnauthorizedAccessException("El rol asignado al usuario no está activo.");

        if (!_hasher.Verify(request.Password, usuario.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        return await GenerarTokensAsync(usuario, session, ct);
    }

    public async Task<AuthTokensResult> RefreshAsync(string refreshToken, SessionContext session, CancellationToken ct)
    {
        var usuario = await BuscarUsuarioPorRefreshTokenAsync(refreshToken, ct);

        var tokenActual = usuario.RefreshTokens
            .FirstOrDefault(t => t.Token == refreshToken && t.EstaActivo)
            ?? throw new UnauthorizedAccessException("Refresh token inválido o expirado.");

        if (!tokenActual.PerteneceASesion(session.SessionFingerprint))
            throw new UnauthorizedAccessException("La sesión del refresh token no coincide con el dispositivo actual.");

        tokenActual.RegistrarUso();
        tokenActual.Revocar();

        return await GenerarTokensAsync(usuario, session, ct);
    }

    public async Task RevocarAsync(string refreshToken, SessionContext session, CancellationToken ct)
    {
        var usuario = await BuscarUsuarioPorRefreshTokenAsync(refreshToken, ct);

        var tokenActual = usuario.RefreshTokens
            .FirstOrDefault(t => t.Token == refreshToken && !t.Revocado)
            ?? throw new UnauthorizedAccessException("Refresh token inválido o expirado.");

        if (!tokenActual.PerteneceASesion(session.SessionFingerprint))
            throw new UnauthorizedAccessException("La sesión del refresh token no coincide con el dispositivo actual.");

        tokenActual.Revocar();
        _usuarioRepo.Update(usuario);
        await _uow.SaveChangesAsync(ct);
    }

    private async Task<AuthTokensResult> GenerarTokensAsync(Usuario usuario, SessionContext session, CancellationToken ct)
    {
        var accessToken = _jwt.GenerateAccessToken(usuario);
        var refreshTokenValue = _jwt.GenerateRefreshToken();
        var refreshExpira = DateTime.UtcNow.AddDays(_refreshTokenDays);

        usuario.RevocarRefreshTokensPorSesion(session.SessionFingerprint);

        var refreshToken = new RefreshToken(
            usuario.Id,
            refreshTokenValue,
            refreshExpira,
            session.DeviceId,
            session.SessionFingerprint,
            session.UserAgent);

        refreshToken.RegistrarUso();
        usuario.AgregarRefreshToken(refreshToken);
        _usuarioRepo.Update(usuario);
        await _usuarioRepo.AddRefreshTokenAsync(refreshToken, ct);
        await _uow.SaveChangesAsync(ct);

        return new AuthTokensResult(
            accessToken.Token,
            refreshTokenValue,
            accessToken.ExpiresAt,
            refreshExpira,
            new UsuarioInfo(
                usuario.Id, usuario.TenantId,
                usuario.RolId,
                usuario.Nombre, usuario.Apellido,
                usuario.Email, usuario.Rol.Nombre, usuario.DebeCambiarPassword));
    }

    private async Task<Usuario> BuscarUsuarioPorRefreshTokenAsync(string token, CancellationToken ct)
    {
        return await _usuarioRepo.GetByRefreshTokenAsync(token, ct)
            ?? throw new UnauthorizedAccessException("Refresh token inválido.");
    }
}
