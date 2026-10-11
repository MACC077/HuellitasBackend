using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Huellitas.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Huellitas.Infrastructure.Services;

public class ProveedorTokens : IProveedorTokens
{
    private readonly IConfiguration _config;

    public ProveedorTokens(IConfiguration config) => _config = config;

    public string GenerarTokenSeguro()
    {
        var secretKey = _config["Jwt:SecretKey"]!;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("purpose", "email_verification")
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}