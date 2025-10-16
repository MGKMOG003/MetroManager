// -------------------- using directives (must be at top) --------------------
using MetroManager.Infrastructure.Data;           // MetroDbContext
using MetroManager.Infrastructure.Identity;       // AppUser
using MetroManager.Web.Extensions;                // IdentitySeedExtensions.SeedIdentityAsync

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using MetroManager.Application.Abstractions;      // IEventRepository, IAnnouncementRepository, ISearchLogRepository
using MetroManager.Application.Services.Events;   // SearchService, RecommendationService, EventsIndex
using MetroManager.Infrastructure.Repositories;   // EventRepository, AnnouncementRepository, SearchLogRepository

// -------------------- host builder --------------------
var builder = WebApplication.CreateBuilder(args);

// Connection
var conn = builder.Configuration.GetConnectionString("Default");

var cs = builder.Configuration.GetConnectionString("Default") ?? "";
try
{
    var sb = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(cs);
    var dataSource = System.IO.Path.GetFullPath(sb.DataSource);
    Console.WriteLine($"[EF] Using SQLite file: {dataSource}");
}
catch { Console.WriteLine($"[EF] Connection: {cs}"); }


// DbContext (single context for Identity + Domain)
builder.Services.AddDbContext<MetroDbContext>(options =>
{
    options.UseSqlite(conn, b => b.MigrationsAssembly("MetroManager.Infrastructure"));
});

// Identity
builder.Services
    .AddDefaultIdentity<AppUser>(opts =>
    {
        opts.SignIn.RequireConfirmedAccount = false;
        opts.Password.RequireDigit = false;
        opts.Password.RequireNonAlphanumeric = false;
        opts.Password.RequireUppercase = false;
        opts.Password.RequiredLength = 6;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MetroDbContext>();

builder.Services.ConfigureApplicationCookie(o =>
{
    o.LoginPath = "/Identity/Account/Login";
    o.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Repositories + Services (Events/Announcements module)
builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IAnnouncementRepository, AnnouncementRepository>();
builder.Services.AddScoped<ISearchLogRepository, SearchLogRepository>();

builder.Services.AddSingleton<EventsIndex>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddSingleton<MetroManager.Web.Services.ClientFingerprint>();

// Helpful EF errors page (dev)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MetroDbContext>();
    var pending = await db.Database.GetPendingMigrationsAsync();
    if (pending.Any())
        await db.Database.MigrateAsync();
}

// Seed roles + admin user (no sign-in)
await IdentitySeedExtensions.SeedIdentityAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// MVC routes
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
