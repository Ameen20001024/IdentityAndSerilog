using IdentityAndSerilog.Domain.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityAndSerilog.Common
{
    public class JwtHelper
    {
        private readonly JwtOptions _JwtOptions;

        public JwtHelper(JwtOptions jwtOptions)
        {
            _JwtOptions = jwtOptions;
        }

        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!)
        };

            // Add role claims
            

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_JwtOptions.Key!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = credentials,
                Issuer = _JwtOptions.Issuer,
                Audience = _JwtOptions.Audience
            };

            return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(randomBytes);
        }
    }
}
