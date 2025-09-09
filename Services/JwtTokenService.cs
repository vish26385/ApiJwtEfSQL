using ApiJwtEfSQL.Models;
using ApiJwtEfSQL.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ApiJwtEfSQL.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthRepository _authRepository;
        public JwtTokenService(IConfiguration configuration, IAuthRepository authRepository)
        {
            _configuration = configuration;
            _authRepository = authRepository;
        }

        public (string token, DateTime expiresAt) CreateToken(User user)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var expires = DateTime.Now.AddMinutes(int.Parse(jwtSection["ExpiresMinutes"]!));
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("uid", user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expires);
        }

        public async Task<string> CreateAndStoreRefreshTokenAsync(User user)
        {
            var refreshtoken = await _authRepository.CreateAndStoreRefreshTokenAsync(user);
            return refreshtoken;
        }

        public async Task<User?> CheckRefreshTokenAsync(string refreshToken)
        {
            var user = await _authRepository.CheckRefreshTokenAsync(refreshToken);
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.Now)
                return null;
            else
                return user;
        }
    }
}
