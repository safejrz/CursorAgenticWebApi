using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CursorAgenticWebApi.Application.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CursorAgenticWebApi.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string SigningKey { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}

public sealed class JwtTokenGenerator(IOptions<JwtOptions> options) : IJwtTokenGenerator
{
    public JwtTokenResult CreateToken(Guid userId, string email, string displayName)
    {
        var o = options.Value;
        if (string.IsNullOrWhiteSpace(o.SigningKey))
            throw new InvalidOperationException("Jwt:SigningKey is not configured.");

        var expires = DateTime.UtcNow.AddMinutes(Math.Max(1, o.ExpiryMinutes));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(o.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("display_name", displayName),
        };

        var token = new JwtSecurityToken(
            issuer: o.Issuer,
            audience: o.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: creds);

        var handler = new JwtSecurityTokenHandler();
        return new JwtTokenResult(handler.WriteToken(token), expires);
    }
}
