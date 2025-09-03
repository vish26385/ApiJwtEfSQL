using ApiJwtEfSQL.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiJwtEfSQL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
