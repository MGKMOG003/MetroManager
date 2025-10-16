using MetroManager.Domain.Entities;
using MetroManager.Infrastructure.Data;
using MetroManager.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MetroManager.Infrastructure.Seed
{
    public static class DbInitializer
    {
        /// <summary>
        /// Seed data that depends only on the DbContext (not on DI services).
        /// Assumes the database schema is already migrated.
        /// </summary>
        public static void EnsureSeed(MetroDbContext db)
        {
            // ---- Events (sample) ----
            if (!db.Events.Any())
            {
                db.Events.Add(new Event
                {
                    Title = "Community Cleanup",
                    Category = "Community",
                    StartsOn = DateTime.Today.AddDays(3).AddHours(9),
                    Venue = "Central Park",
                    City = "Metro City",
                    Description = "Join us for a community clean-up."
                });
            }

            // ---- Announcements (sample) ----
            if (!db.Announcements.Any())
            {
                db.Announcements.AddRange(
                    new Announcement
                    {
                        Title = "Planned Water Maintenance",
                        Summary = "Water supply interruption on 15 Oct, 09:00–13:00 in Ward 4.",
                        Category = "Outage",
                        IsPinned = true,
                        PublishedUtc = DateTime.UtcNow.AddDays(-3)
                    },
                    new Announcement
                    {
                        Title = "Community Clean-up Day",
                        Summary = "This Saturday 08:00, Civic Centre parking.",
                        Category = "Event",
                        PublishedUtc = DateTime.UtcNow.AddDays(-1)
                    },
                    new Announcement
                    {
                        Title = "Load-shedding Advisory",
                        Summary = "Stage 2 expected this evening.",
                        Category = "Notice",
                        PublishedUtc = DateTime.UtcNow.AddHours(-6)
                    }
                );
            }

            db.SaveChanges();
        }

        /// <summary>
        /// Seed Identity (roles + demo users + anonymous bucket).
        /// </summary>
        private const string AnonEmail = "anonymous@system.local";

        public static async Task EnsureIdentitySeedAsync(IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();

            // Roles
            foreach (var role in new[] { "Admin", "Public" })
                if (!await roleManager.RoleExistsAsync(role))
                    _ = await roleManager.CreateAsync(new IdentityRole(role));

            // Admin demo
            var adminEmail = "admin@demo.local";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new AppUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    DisplayName = "Demo Admin",
                    EmailConfirmed = true
                };
                if ((await userManager.CreateAsync(admin, "Admin123$")).Succeeded)
                    await userManager.AddToRoleAsync(admin, "Admin");
            }
            else if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Public demo
            var publicEmail = "public@demo.local";
            var citizen = await userManager.FindByEmailAsync(publicEmail);
            if (citizen == null)
            {
                citizen = new AppUser
                {
                    UserName = publicEmail,
                    Email = publicEmail,
                    DisplayName = "Demo Citizen",
                    EmailConfirmed = true
                };
                if ((await userManager.CreateAsync(citizen, "Public123$")).Succeeded)
                    await userManager.AddToRoleAsync(citizen, "Public");
            }
            else if (!await userManager.IsInRoleAsync(citizen, "Public"))
            {
                await userManager.AddToRoleAsync(citizen, "Public");
            }

            // Anonymous bucket user (no one logs in with this)
            var anon = await userManager.FindByEmailAsync(AnonEmail);
            if (anon == null)
            {
                anon = new AppUser
                {
                    UserName = AnonEmail,
                    Email = AnonEmail,
                    DisplayName = "Anonymous Reporter",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(anon, "Anon!23456_pass_ONLY_SEED");
                await userManager.AddToRoleAsync(anon, "Public");
            }
        }
    }
}
