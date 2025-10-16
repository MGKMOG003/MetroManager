using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MetroManager.Infrastructure.Identity;

namespace MetroManager.Infrastructure.Persistence
{
    public class AppIdentityDbContext : IdentityDbContext<AppUser>
    {
        public AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ensure DisplayName column exists / sized
            builder.Entity<AppUser>(b =>
            {
                b.Property(u => u.DisplayName).HasMaxLength(128);
            });
        }
    }
}
