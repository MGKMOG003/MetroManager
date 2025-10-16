using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MetroManager.Infrastructure.Data;
using MetroManager.Infrastructure.Identity;
using MetroManager.Domain.Entities;

namespace MetroManager.Web.Extensions
{
    public static class IdentitySeedExtensions
    {
        public static async Task SeedIdentityAsync(IServiceProvider services)
        {
            // make sure DB is created/migrated before seeding identity & domain data
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MetroDbContext>();
            await db.Database.MigrateAsync();

            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            // 1) Roles
            async Task EnsureRole(string role)
            {
                if (!await roleMgr.RoleExistsAsync(role))
                    _ = await roleMgr.CreateAsync(new IdentityRole(role));
            }
            await EnsureRole("Admin");
            await EnsureRole("Client");

            // 2) Admin user
            const string adminEmail = "admin@metro.local";
            const string adminPass = "P@ssw0rd!23";

            var admin = await userMgr.FindByEmailAsync(adminEmail);
            if (admin is null)
            {
                admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    DisplayName = "System Administrator",
                    FullName = "System Administrator"
                };
                var create = await userMgr.CreateAsync(admin, adminPass);
                if (!create.Succeeded)
                    throw new InvalidOperationException("Failed to create seed admin: " +
                                                        string.Join("; ", create.Errors.Select(e => e.Description)));
            }

            if (!await userMgr.IsInRoleAsync(admin, "Admin"))
                await userMgr.AddToRoleAsync(admin, "Admin");

            // 3) Domain data (idempotent)
            await SeedDomainAsync(db);
        }

        private static async Task SeedDomainAsync(MetroDbContext db)
        {
            // ----- Announcements (2) -----
            if (!await db.Announcements.AnyAsync())
            {
                db.Announcements.AddRange(
                    new Announcement
                    {
                        Title = "Water Outage – Ward 8",
                        Summary = "Routine maintenance will affect water supply in Ward 8, 09:00–14:00.",
                        Category = "Utilities",
                        IsPinned = true,
                        PublishedUtc = DateTime.UtcNow.AddDays(-2),
                        ExpiresUtc = DateTime.UtcNow.AddDays(5),
                        Body = "Technicians will be working on the main valves. Please store water ahead of time.",
                        Link = "https://metro.example/water-maintenance",
                        MediaUrl = null
                    },
                    new Announcement
                    {
                        Title = "Road Closure – Main Street",
                        Summary = "Main St closed Sat 06:00–18:00 for parade. Use Oak Ave detour.",
                        Category = "Traffic",
                        IsPinned = false,
                        PublishedUtc = DateTime.UtcNow.AddDays(-1),
                        ExpiresUtc = DateTime.UtcNow.AddDays(1),
                        Body = "Parade route will span Civic Square to Town Hall. Expect delays in adjacent roads.",
                        Link = "https://metro.example/road-closure",
                        MediaUrl = null
                    }
                );
                await db.SaveChangesAsync();
            }

            // ----- Events (3) -----
            if (!await db.Events.AnyAsync())
            {
                db.Events.AddRange(
                    new Event
                    {
                        Title = "Community Clean-up Day",
                        Category = "Community",
                        City = "Metro City",
                        Venue = "Central Park",
                        LocationAddress = "1 Park Rd, Central",
                        Latitude = -26.2041,
                        Longitude = 28.0473,
                        StartsOn = DateTime.UtcNow.AddDays(3).Date.AddHours(8),
                        EndsOn = DateTime.UtcNow.AddDays(3).Date.AddHours(12),
                        EntryPrice = 0m,
                        Url = "https://metro.example/cleanup",
                        MediaUrl = null,
                        Description = "Join neighbours in keeping our parks clean.",
                        TagsCsv = "volunteer,parks,cleanup",
                        AgeRestriction = null
                    },
                    new Event
                    {
                        Title = "Ward 4 Town Hall",
                        Category = "Civic",
                        City = "Metro City",
                        Venue = "Civic Centre – Hall A",
                        LocationAddress = "100 Government Ave",
                        Latitude = -26.205,
                        Longitude = 28.049,
                        StartsOn = DateTime.UtcNow.AddDays(7).Date.AddHours(18),
                        EndsOn = DateTime.UtcNow.AddDays(7).Date.AddHours(20),
                        EntryPrice = 0m,
                        Url = "https://metro.example/town-hall",
                        MediaUrl = null,
                        Description = "Open forum with local representatives.",
                        TagsCsv = "townhall,civic,community",
                        AgeRestriction = null
                    },
                    new Event
                    {
                        Title = "Spring Market",
                        Category = "Recreation",
                        City = "Metro City",
                        Venue = "Riverside Promenade",
                        LocationAddress = "Riverside Blvd",
                        Latitude = -26.206,
                        Longitude = 28.05,
                        StartsOn = DateTime.UtcNow.AddDays(12).Date.AddHours(10),
                        EndsOn = DateTime.UtcNow.AddDays(12).Date.AddHours(16),
                        EntryPrice = 20m,
                        Url = "https://metro.example/market",
                        MediaUrl = null,
                        Description = "Local crafts, food trucks, and live music.",
                        TagsCsv = "market,music,food",
                        AgeRestriction = null
                    }
                );
                await db.SaveChangesAsync();
            }
        }
    }
}
