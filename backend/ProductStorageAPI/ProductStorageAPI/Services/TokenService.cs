using Microsoft.IdentityModel.Tokens;
using ProductStorageAPI.Abstractions;
using ProductStorageAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ProductStorageAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TokenModel GenerateToken(User user)
        {
            var expiresInMinutes = _configuration.GetValue<int>("Jwt:ExpiresInMinutes");
            var expiresAt = DateTime.Now.AddMinutes(expiresInMinutes);

            var claims = new List<Claim>
            {
                new (JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new (JwtRegisteredClaimNames.Email, user.Email),
                new (ClaimTypes.Name, user.FullName),
                new (ClaimTypes.Role, user.Role),
                new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return new TokenModel
            {
                Token = tokenString,
                ExpiresAt = expiresAt
            };
        }
    }
}
