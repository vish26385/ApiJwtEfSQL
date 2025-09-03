using ApiJwtEfSQL.Data;
using ApiJwtEfSQL.Models;
using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace ApiJwtEfSQL.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;
        private readonly string _connectionstring;
        public AuthRepository(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionstring = configuration.GetConnectionString("ConnectionString");
        }
        public async Task<int> RegisterAsync(string username, string password, string role)
        {
            var passwordHasher = new PasswordHasher<User>();
            var user = new User { Username = username };
            var passwordHash = passwordHasher.HashPassword(user, password);

            await using var con = new SqlConnection(_connectionstring);
            if (con.State != System.Data.ConnectionState.Open) await con.OpenAsync();

            using var cmd = new SqlCommand("sp_InsertUser", con);
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@Username", username);
            cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
            cmd.Parameters.AddWithValue("@Userrole", role);
            cmd.Parameters.Add("@NewUserId", SqlDbType.Int).Direction = ParameterDirection.Output;
            await cmd.ExecuteNonQueryAsync();
            var newUserId = (int)cmd.Parameters["@NewUserId"].Value;
            if (newUserId == -1)
            {
                throw new InvalidOperationException("Username already exists.");
            }
            return newUserId;
        }

        public async Task<User?> AuthenticateAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
            var success = false;
            if (user != null)
            {
                var passwordHasher = new PasswordHasher<User>();
                var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

                if (result == PasswordVerificationResult.Success)
                {
                    success = true;
                }
                else
                    success = false;
            }
            else
                success = false;

            if (!success) return null;

            var userId = user?.Id;
            return await _context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == userId);
        }

        public async Task<string> CreateAndStoreRefreshTokenAsync(User user)
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            user.RefreshToken = Convert.ToBase64String(randomNumber);
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            var existinguser = await _context.Users.FindAsync(user.Id);
            if (existinguser != null)
            {
                _context.Entry(existinguser).State = EntityState.Detached;
            }
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<User?> CheckRefreshTokenAsync(string refreshToken)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
            return user;
        }
    }
}
