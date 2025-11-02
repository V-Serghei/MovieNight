using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Core.Security;


public sealed class JwtTokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly SymmetricSecurityKey _key;

    public JwtTokenService(IConfiguration cfg)
    {
        _issuer   = cfg["AUTH_JWT_ISSUER"]   ?? "MovieNight.Auth";
        _audience = cfg["AUTH_JWT_AUDIENCE"] ?? "MovieNight.Client";
        var secret = cfg["AUTH_JWT_SECRET"]  ?? "dev-super-secret-change-me-but-32+bytes";

        byte[] keyBytes;
        const string b64Prefix = "base64:";
        if (secret.StartsWith(b64Prefix, StringComparison.OrdinalIgnoreCase))
            keyBytes = Convert.FromBase64String(secret[b64Prefix.Length..]);
        else
            keyBytes = Encoding.UTF8.GetBytes(secret);

        if (keyBytes.Length < 32)
            throw new InvalidOperationException("AUTH_JWT_SECRET must be at least 32 bytes (256 bits).");

        _key = new SymmetricSecurityKey(keyBytes);
    }

    public (string token, DateTimeOffset exp) CreateAccessToken(Guid userId, string email, TimeSpan? lifetime = null)
    {
        var now = DateTimeOffset.UtcNow;
        var exp = now.Add(lifetime ?? TimeSpan.FromMinutes(30));
        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email)
            },
            notBefore: now.UtcDateTime,
            expires: exp.UtcDateTime,
            signingCredentials: creds
        );

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);
        return (token, exp);
    }

    public TokenValidationParameters GetValidationParameters() => new()
    {
        ValidateIssuer = true, ValidIssuer = _issuer,
        ValidateAudience = true, ValidAudience = _audience,
        ValidateIssuerSigningKey = true, IssuerSigningKey = _key,
        ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
    };
}