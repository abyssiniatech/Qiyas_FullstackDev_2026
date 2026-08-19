using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TmsApi.Infrastructure.Identity;

namespace TmsApi.Infrastructure.Services;

public class TokenService
{
    private readonly IConfiguration _config;
    public TokenService(IConfiguration config)
    {
        _config = config;
    }
    public string GenerateJwt(TmsUser user, IList<string> roles)
    {
        var payload = new Dictionary<string, object?>
{
{ "nameid", user.Id },
{ "email", user.Email ?? string.Empty },
{ "FirstName", user.FirstName },
{ "role", roles },
{ "iss", _config["Jwt:Issuer"] },
{ "aud", _config["Jwt:Audience"] },
{ "exp", DateTimeOffset.UtcNow.AddMinutes(
    int.Parse(_config["Jwt:ExpiryMinutes"]!)).ToUnixTimeSeconds() }
};

        static string Base64UrlEncode(byte[] bytes) => Convert.ToBase64String(bytes)
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');

        var header = Base64UrlEncode(Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" })));
        var encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(payload)));
        var unsignedToken = $"{header}.{encodedPayload}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var signature = Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken)));
        return $"{unsignedToken}.{signature}";
    }
}