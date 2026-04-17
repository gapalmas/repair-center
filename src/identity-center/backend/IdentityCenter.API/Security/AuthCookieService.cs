using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace IdentityCenter.API.Security;

public sealed class AuthCookieService
{
    public const string RefreshTokenCookieName = "ic_refresh_token";
    public const string CsrfTokenCookieName = "ic_csrf_token";
    public const string CsrfHeaderName = "X-CSRF-TOKEN";

    private readonly HashSet<string> _allowedOrigins;

    public AuthCookieService(IOptions<AuthSecurityOptions> options)
    {
        _allowedOrigins = options.Value.AllowedOrigins
            .Where(static origin => !string.IsNullOrWhiteSpace(origin))
            .Select(NormalizeOrigin)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }

    public string SetAuthCookies(HttpResponse response, string refreshToken, DateTime refreshTokenExpiresAt)
    {
        var csrfToken = GenerateToken();

        response.Cookies.Append(
            RefreshTokenCookieName,
            refreshToken,
            BuildRefreshCookieOptions(refreshTokenExpiresAt));

        response.Cookies.Append(
            CsrfTokenCookieName,
            csrfToken,
            BuildCsrfCookieOptions(refreshTokenExpiresAt));

        response.Headers[CsrfHeaderName] = csrfToken;

        return csrfToken;
    }

    public string? GetRefreshToken(HttpRequest request)
        => request.Cookies[RefreshTokenCookieName];

    public bool IsCsrfTokenValid(HttpRequest request)
    {
        var csrfCookie = request.Cookies[CsrfTokenCookieName];
        var csrfHeader = request.Headers[CsrfHeaderName].ToString();

        if (string.IsNullOrWhiteSpace(csrfCookie) || string.IsNullOrWhiteSpace(csrfHeader))
            return false;

        var cookieBytes = Encoding.UTF8.GetBytes(csrfCookie);
        var headerBytes = Encoding.UTF8.GetBytes(csrfHeader);

        return CryptographicOperations.FixedTimeEquals(cookieBytes, headerBytes);
    }

    public bool IsOriginAllowed(HttpRequest request)
    {
        if (_allowedOrigins.Count == 0)
            return true;

        var candidate = request.Headers.Origin.ToString();

        if (string.IsNullOrWhiteSpace(candidate) &&
            Uri.TryCreate(request.Headers.Referer.ToString(), UriKind.Absolute, out var refererUri))
        {
            candidate = refererUri.GetLeftPart(UriPartial.Authority);
        }

        if (string.IsNullOrWhiteSpace(candidate))
            return false;

        return _allowedOrigins.Contains(NormalizeOrigin(candidate));
    }

    public void ClearAuthCookies(HttpResponse response)
    {
        response.Cookies.Delete(RefreshTokenCookieName, BuildRefreshCookieOptions(DateTime.UnixEpoch));
        response.Cookies.Delete(CsrfTokenCookieName, BuildCsrfCookieOptions(DateTime.UnixEpoch));
    }

    private static CookieOptions BuildRefreshCookieOptions(DateTime expiresAt) => new()
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = expiresAt,
        Path = "/"
    };

    private static CookieOptions BuildCsrfCookieOptions(DateTime expiresAt) => new()
    {
        HttpOnly = false,
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Expires = expiresAt,
        Path = "/"
    };

    private static string GenerateToken()
        => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    private static string NormalizeOrigin(string origin)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
            return origin.Trim().TrimEnd('/');

        return uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }
}