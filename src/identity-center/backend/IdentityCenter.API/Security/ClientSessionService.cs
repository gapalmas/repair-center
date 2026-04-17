using System.Security.Cryptography;
using System.Text;
using IdentityCenter.Application.DTOs.Auth;

namespace IdentityCenter.API.Security;

public sealed class ClientSessionService
{
    public const string DeviceIdHeaderName = "X-Device-Id";
    private const string DefaultDeviceId = "swagger-browser";

    public SessionContext Create(HttpRequest request)
    {
        var deviceId = request.Headers[DeviceIdHeaderName].ToString();
        if (string.IsNullOrWhiteSpace(deviceId))
            deviceId = DefaultDeviceId;

        var userAgent = request.Headers.UserAgent.ToString();
        if (string.IsNullOrWhiteSpace(userAgent))
            userAgent = "unknown-user-agent";

        var fingerprint = ComputeFingerprint(deviceId, userAgent);

        return new SessionContext(deviceId, fingerprint, userAgent);
    }

    private static string ComputeFingerprint(string deviceId, string userAgent)
    {
        var input = $"{deviceId}|{userAgent}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash);
    }
}