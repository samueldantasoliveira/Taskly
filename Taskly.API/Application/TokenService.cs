using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Taskly.Domain.Entities;

public interface ITokenService
{
    string GenerateToken(User user, out DateTime expiresAt);
}

public class TokenService : ITokenService
{
    private readonly byte[] _key;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiresMinutes;

    public TokenService(IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Jwt");

        _key = Encoding.UTF8.GetBytes(
            jwt["Key"] ??
            throw new InvalidOperationException("JWT Key is missing in configuration.")
        );

        _issuer = jwt["Issuer"] ??
            throw new InvalidOperationException("JWT Issuer is missing in configuration.");

        _audience = jwt["Audience"] ??
            throw new InvalidOperationException("JWT Audience is missing in configuration.");

        _expiresMinutes = int.Parse(jwt["ExpiresMinutes"] ?? "60");
    }

    public string GenerateToken(User user, out DateTime expiresAt)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("name", user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(_key),
            SecurityAlgorithms.HmacSha256
        );

        expiresAt = DateTime.UtcNow.AddMinutes(_expiresMinutes);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
