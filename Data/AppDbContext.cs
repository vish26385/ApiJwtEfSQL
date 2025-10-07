using ApiJwtEfSQL.Models;
using Microsoft.EntityFrameworkCore;
using Task = ApiJwtEfSQL.Models.Task;

namespace ApiJwtEfSQL.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Product> Products { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Task> Tasks { get; set; }
    }
}
