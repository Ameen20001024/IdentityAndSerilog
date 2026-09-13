using IdentityAndSerilog.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IdentityAndSerilog.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>

    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<RefreshTokens> RefreshTokens { get; set; }

        
    }
}
