// MetroManager.Web/Extensions/SeedExtensions.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MetroManager.Infrastructure.Data;
using MetroManager.Infrastructure.Identity;

namespace MetroManager.Web.Extensions
{
    public static class SeedExtensions
    {
        /// <summary>
        /// Dev-only helper: wipe domain data and non-admin users.
        /// Call from Program.cs guarded by if (app.Environment.IsDevelopment()).
        /// </summary>
        public static async Task WipeDevDataAsync(this IServiceProvider services, string adminEmailToKeep)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MetroDbContext>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            // domain data – order matters due to FKs
            await db.Attachments.ExecuteDeleteAsync();
            await db.StatusHistories.ExecuteDeleteAsync();
            await db.ServiceRequests.ExecuteDeleteAsync();
            await db.Issues.ExecuteDeleteAsync();
            await db.Announcements.ExecuteDeleteAsync();
            await db.Events.ExecuteDeleteAsync();

            // identity users – keep only the seeded admin
            var users = await userMgr.Users.Where(u => u.Email != adminEmailToKeep).ToListAsync();
            foreach (var u in users)
                await userMgr.DeleteAsync(u);
        }
    }
}
