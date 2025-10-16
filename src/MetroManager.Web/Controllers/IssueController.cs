using MetroManager.Domain.Entities;
using MetroManager.Domain.Enums;
using MetroManager.Infrastructure.Data;
using MetroManager.Infrastructure.Identity;
using MetroManager.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetroManager.Web.Controllers
{
    [Authorize(Roles = "Client")]
    public class IssueController : Controller
    {
        private readonly MetroDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        // Category → Subcategories
        private static readonly Dictionary<string, List<string>> _map = new()
        {
            ["Roads & Transport"] = new() { "Pothole", "Traffic Signal Fault", "Street Name Missing", "Sidewalk Damage", "Road Markings Faded", "Illegal Speed Hump" },
            ["Water & Sanitation"] = new() { "Burst Pipe", "Water Leak", "Blocked Drain", "Sewage Overflow", "No Water", "Water Quality" },
            ["Electricity"] = new() { "Power Outage", "Exposed Cables", "Meter Fault", "Streetlight Out", "Substation Noise" },
            ["Waste Management"] = new() { "Missed Collection", "Illegal Dumping", "Overflowing Bin", "Recycling", "Littering Hotspot" },
            ["Parks & Recreation"] = new() { "Grass Overgrown", "Tree Trimming", "Playground Damage", "Park Lighting", "Vandalism" },
            ["Public Safety"] = new() { "Graffiti", "Broken Fence", "Open Manhole", "Abandoned Vehicle", "Vandalism" },
            ["By-Law / Compliance"] = new() { "Noise Complaint", "Informal Trading", "Illegal Advertising", "Building Without Permit", "Land Use Violation" },
            ["Health & Environmental"] = new() { "Air Pollution", "Water Pollution", "Pest Infestation", "Dead Animal Removal" },
            ["Housing"] = new() { "RDP Enquiry", "Maintenance", "Allocation Issue", "Eviction Notice" },
            ["Community Services"] = new() { "Library", "Clinic", "Community Hall Booking", "Social Services" },
            ["Licensing & Permits"] = new() { "Business License", "Building Permit", "Event Permit", "Fire Safety Certificate" },
            ["Other"] = new() { }
        };

        private static readonly List<string> _categories = _map.Keys.ToList();

        public IssueController(MetroDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private void HydrateLists(IssueCreateVm vm)
        {
            vm.Categories = _categories;
            vm.CategorySubcategories = _map;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var vm = new IssueCreateVm();
            HydrateLists(vm);
            return View(vm);
        }
        public async Task<IActionResult> Retract(int id, [FromServices] UserManager<AppUser> userManager)
        {
            var me = userManager.GetUserId(User);
            var issue = await _db.Issues.FirstOrDefaultAsync(i => i.Id == id);
            if (issue == null) return NotFound();
            if (issue.CreatedByUserId != me) return Forbid();

            if (issue.Status != IssueStatus.New)
            {
                TempData["Err"] = "You can only retract a new/unprocessed ticket.";
                return RedirectToAction("MyReports");
            }

            _db.Issues.Remove(issue);
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Ticket retracted.";
            return RedirectToAction("MyReports");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IssueCreateVm vm)
        {
            HydrateLists(vm);
            if (!ModelState.IsValid) return View(vm);

            var userId = _userManager.GetUserId(User);

            var issue = new Issue();
            var now = DateTime.UtcNow;

            // helper: set a property only if it exists (and convert basic types)
            static void SetProp(object target, string name, object? value)
            {
                var p = target.GetType().GetProperty(name);
                if (p is null || !p.CanWrite) return;

                var targetType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;
                try
                {
                    object? v = value;
                    if (v != null && !targetType.IsAssignableFrom(v.GetType()))
                        v = Convert.ChangeType(v, targetType);
                    p.SetValue(target, v);
                }
                catch
                {
                    // no-throw: skip if conversion fails
                }
            }

            // Generate a PublicId value suitable for string/int/Guid
            async Task<object> GeneratePublicIdAsync(Type targetType)
            {
                var t = Nullable.GetUnderlyingType(targetType) ?? targetType;

                if (t == typeof(string))
                {
                    // e.g., MM-YYYYMMDD-XXXX (ensure low collision)
                    string prefix = $"MM-{now:yyyyMMdd}-";
                    string candidate;
                    int salt = 0;
                    do
                    {
                        candidate = prefix + Random.Shared.Next(1000, 9999 + salt).ToString("D4");
                        salt++;
                    }
                    while (await _db.Issues.AnyAsync(i => EF.Property<string>(i, "PublicId") == candidate));
                    return candidate;
                }
                if (t == typeof(Guid))
                {
                    return Guid.NewGuid();
                }
                if (t == typeof(long))
                {
                    // time-based number
                    return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
                if (t == typeof(int))
                {
                    // fit within int32
                    return unchecked((int)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() % int.MaxValue));
                }
                // fallback to string
                return $"MM-{now:yyyyMMdd}-{Random.Shared.Next(1000, 9999):D4}";
            }

            // Map VM → Issue (only if props exist)
            SetProp(issue, "Title", vm.Title);
            SetProp(issue, "Summary", vm.Title);
            SetProp(issue, "Subject", vm.Title);

            SetProp(issue, "Description", vm.Description);
            SetProp(issue, "Details", vm.Description);

            SetProp(issue, "Category", vm.Category);
            var finalSub = string.IsNullOrWhiteSpace(vm.Subcategory) && vm.Category == "Other"
                ? vm.OtherCategory
                : vm.Subcategory;
            SetProp(issue, "Subcategory", finalSub);

            SetProp(issue, "LocationText", vm.LocationText);
            SetProp(issue, "LocationDescription", vm.LocationText);
            SetProp(issue, "LocationLink", vm.LocationLink);
            SetProp(issue, "MapUrl", vm.LocationLink);

            SetProp(issue, "CreatedByUserId", userId);
            SetProp(issue, "OwnerUserId", userId);
            SetProp(issue, "ReporterUserId", userId);
            SetProp(issue, "IsAnonymous", false);

            SetProp(issue, "Status", "New");
            SetProp(issue, "CreatedOn", now);
            SetProp(issue, "CreatedAt", now);
            SetProp(issue, "ReportedAt", now);

            // Ensure PublicId is set if the entity has such a property
            var publicIdProp = issue.GetType().GetProperty("PublicId");
            if (publicIdProp != null && publicIdProp.CanWrite)
            {
                var val = await GeneratePublicIdAsync(publicIdProp.PropertyType);
                SetProp(issue, "PublicId", val);
            }

            _db.Issues.Add(issue);
            await _db.SaveChangesAsync();

            TempData["IssueSubmitted"] = "1";
            return View("Submitted"); // shows animation then redirects to Dashboard
        }

        // Optional: "My Reports"
        [HttpGet]
        public async Task<IActionResult> MyReports()
        {
            var userId = _userManager.GetUserId(User);

            var mine = await _db.Issues
                .Where(i => EF.Property<string>(i, "CreatedByUserId") == userId)
                .OrderByDescending(i => EF.Property<object>(i, "Id"))
                .ToListAsync();

            return View(mine);
        }
    }
}
