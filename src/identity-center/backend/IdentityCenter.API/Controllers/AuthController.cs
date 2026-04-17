using IdentityCenter.Application.DTOs.Auth;
using IdentityCenter.Application.Services;
using IdentityCenter.API.Security;
using Microsoft.AspNetCore.Mvc;

namespace IdentityCenter.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _auth;
    private readonly AuthCookieService _cookies;
    private readonly ClientSessionService _sessions;

    public AuthController(AuthService auth, AuthCookieService cookies, ClientSessionService sessions)
    {
        _auth = auth;
        _cookies = cookies;
        _sessions = sessions;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var session = _sessions.Create(Request);
        var result = await _auth.LoginAsync(request, session, ct);
        var csrfToken = _cookies.SetAuthCookies(Response, result.RefreshToken, result.RefreshTokenExpiresAt);

        return Ok(new AuthResponse(
            result.AccessToken,
            result.ExpiresAt,
            result.Usuario,
            csrfToken));
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        if (!_cookies.IsOriginAllowed(Request))
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Origin o Referer no permitido." });

        if (!_cookies.IsCsrfTokenValid(Request))
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Token CSRF inválido." });

        var refreshToken = _cookies.GetRefreshToken(Request)
            ?? throw new UnauthorizedAccessException("Refresh token no encontrado en cookie segura.");

        var session = _sessions.Create(Request);
        var result = await _auth.RefreshAsync(refreshToken, session, ct);
        var csrfToken = _cookies.SetAuthCookies(Response, result.RefreshToken, result.RefreshTokenExpiresAt);

        return Ok(new AuthResponse(
            result.AccessToken,
            result.ExpiresAt,
            result.Usuario,
            csrfToken));
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke(CancellationToken ct)
    {
        if (!_cookies.IsOriginAllowed(Request))
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Origin o Referer no permitido." });

        if (!_cookies.IsCsrfTokenValid(Request))
            return StatusCode(StatusCodes.Status403Forbidden, new { error = "Token CSRF inválido." });

        var refreshToken = _cookies.GetRefreshToken(Request)
            ?? throw new UnauthorizedAccessException("Refresh token no encontrado en cookie segura.");

        var session = _sessions.Create(Request);
        await _auth.RevocarAsync(refreshToken, session, ct);
        _cookies.ClearAuthCookies(Response);

        return NoContent();
    }
}
