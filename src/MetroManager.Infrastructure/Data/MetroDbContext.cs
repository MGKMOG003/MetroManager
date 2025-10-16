using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using MetroManager.Infrastructure.Identity;
using MetroManager.Domain.Entities;
using MetroManager.Domain.Enums; // IssueStatus

namespace MetroManager.Infrastructure.Data
{
    /// <summary>
    /// Single DbContext hosting BOTH ASP.NET Identity tables and domain tables.
    /// </summary>
    public class MetroDbContext : IdentityDbContext<AppUser, IdentityRole, string>
    {
        public MetroDbContext(DbContextOptions<MetroDbContext> options) : base(options) { }

        // Domain sets
        public DbSet<Issue> Issues => Set<Issue>();
        public DbSet<Attachment> Attachments => Set<Attachment>();
        public DbSet<ServiceRequest> ServiceRequests => Set<ServiceRequest>();
        public DbSet<StatusHistory> StatusHistories => Set<StatusHistory>();

        public DbSet<Event> Events => Set<Event>();
        public DbSet<Announcement> Announcements => Set<Announcement>();

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // ISSUE
            b.Entity<Issue>(e =>
            {
                e.HasKey(x => x.Id);

                e.HasIndex(x => x.PublicId).IsUnique();
                e.Property(x => x.PublicId).IsRequired().HasMaxLength(64);

                e.Property(x => x.Category).IsRequired().HasMaxLength(80);
                e.Property(x => x.Subcategory).HasMaxLength(80);

                e.Property(x => x.LocationText).IsRequired().HasMaxLength(400);
                e.Property(x => x.LocationLink).HasMaxLength(512);

                e.Property(x => x.Description).IsRequired().HasMaxLength(2000);

                e.Property(x => x.IsAnonymous).HasDefaultValue(false);

                e.Property(x => x.CreatedByUserId).HasMaxLength(256);
                e.Property(x => x.CreatedUtc).IsRequired();

                e.Property(x => x.Status)
                 .HasConversion<int>()
                 .IsRequired()
                 .HasDefaultValue(IssueStatus.New);

                e.Property(x => x.AdminNotes).HasMaxLength(2000);
                e.Property(x => x.UpdatedUtc);

                // optional FK to AspNetUsers
                e.HasOne<AppUser>()
                 .WithMany()
                 .HasForeignKey(x => x.CreatedByUserId)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasMany(x => x.Attachments)
                 .WithOne(a => a.Issue)
                 .HasForeignKey(a => a.IssueId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(x => x.ServiceRequest)
                 .WithOne(sr => sr.Issue)
                 .HasForeignKey<ServiceRequest>(sr => sr.IssueId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ATTACHMENT
            b.Entity<Attachment>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.FileName).IsRequired().HasMaxLength(260);
                e.Property(x => x.StoredPath).IsRequired().HasMaxLength(512);
                e.Property(x => x.ContentType).IsRequired().HasMaxLength(128);
            });

            // SERVICEREQUEST
            b.Entity<ServiceRequest>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasIndex(x => x.RequestCode).IsUnique();
                e.Property(x => x.RequestCode).IsRequired().HasMaxLength(64);
                e.Property(x => x.CurrentStatus).IsRequired().HasMaxLength(64);
            });

            // STATUSHISTORY
            b.Entity<StatusHistory>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Status).IsRequired().HasMaxLength(64);
                e.Property(x => x.ChangedUtc).IsRequired();

                e.HasOne(x => x.ServiceRequest)
                 .WithMany(sr => sr.History)
                 .HasForeignKey(x => x.ServiceRequestId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // EVENT (Part 2)
            b.Entity<Event>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Title).IsRequired().HasMaxLength(200);
                e.Property(x => x.Category).IsRequired().HasMaxLength(64);
                e.Property(x => x.StartsOn).IsRequired();
                e.Property(x => x.Description).IsRequired();

                e.Property(x => x.EndsOn);
                e.Property(x => x.Venue).HasMaxLength(160);
                e.Property(x => x.City).HasMaxLength(128);
                e.Property(x => x.TagsCsv).HasMaxLength(256);
                e.Property(x => x.Url).HasMaxLength(1024);

                e.Property(x => x.LocationAddress).HasMaxLength(256);
                e.Property(x => x.Latitude);
                e.Property(x => x.Longitude);
                e.Property(x => x.EntryPrice).HasColumnType("decimal(10,2)");
                e.Property(x => x.AgeRestriction).HasMaxLength(32);
                e.Property(x => x.MediaUrl).HasMaxLength(1024);
            });

            // ANNOUNCEMENT (Part 2)
            b.Entity<Announcement>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Title).IsRequired().HasMaxLength(160);
                e.Property(x => x.Summary).HasMaxLength(500);
                e.Property(x => x.Body);
                e.Property(x => x.Category).IsRequired().HasMaxLength(64);
                e.Property(x => x.IsPinned).HasDefaultValue(false);
                e.Property(x => x.PublishedUtc).IsRequired();
                e.Property(x => x.ExpiresUtc);

                e.Property(x => x.Link).HasMaxLength(1024);
                e.Property(x => x.MediaUrl).HasMaxLength(1024);
            });
        }
    }
}
