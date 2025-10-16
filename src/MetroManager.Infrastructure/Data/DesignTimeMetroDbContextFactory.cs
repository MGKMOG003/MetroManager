using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MetroManager.Infrastructure.Data
{
    // Lets 'dotnet ef' create the context without booting the web host.
    public sealed class DesignTimeMetroDbContextFactory : IDesignTimeDbContextFactory<MetroDbContext>
    {
        public MetroDbContext CreateDbContext(string[] args)
        {
            var conn = System.Environment.GetEnvironmentVariable("MM_SQLITE")
                      ?? "Data Source=community.db";

            var options = new DbContextOptionsBuilder<MetroDbContext>()
                .UseSqlite(conn, b => b.MigrationsAssembly("MetroManager.Infrastructure"))
                .Options;

            return new MetroDbContext(options);
        }
    }
}
