using ApiJwtEfSQL.Models;

namespace ApiJwtEfSQL.Services
{
    public interface IJwtTokenService
    {
        (string token, DateTime expiresAt) CreateToken(User user);

        Task<string> CreateAndStoreRefreshTokenAsync(User user);

        Task<User?> CheckRefreshTokenAsync(string refreshToken);
    }
}
