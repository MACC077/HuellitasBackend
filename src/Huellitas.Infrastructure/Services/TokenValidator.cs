using Huellitas.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Huellitas.Infrastructure.Services
{
    /// <summary>
    /// Servicio para validar tokens de seguridad.
    /// </summary>
    public class TokenValidator : ITokenValidador
    {
        private readonly IConfiguration _config;

        public TokenValidator(IConfiguration config) => _config = config;

        public bool ValidarToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            var secretKey = _config["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Falta Jwt:SecretKey.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out _);

                return true;
            }
            catch (SecurityTokenException)
            {
                // Token inválido: firma, expiración, issuer, audience...
                return false;
            }
            catch (ArgumentException)
            {
                // Token mal formado (no es un JWT válido)
                return false;
            }
        }
    }
}
