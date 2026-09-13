using IdentityAndSerilog.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityAndSerilog.Data
{
    public class AppDbContext : IdentityDbContext<User>

    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<RefreshTokens> RefreshTokens { get; set; }

        
    }
}
