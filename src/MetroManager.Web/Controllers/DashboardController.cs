using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MetroManager.Infrastructure.Data;
using MetroManager.Infrastructure.Identity;
using MetroManager.Web.ViewModels.Dashboard;
using MetroManager.Domain.Entities; // for Issue/Announcement types

namespace MetroManager.Web.Controllers
{
    [Authorize(Roles = "Client")]
    public class DashboardController : Controller
    {
        private readonly MetroDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public DashboardController(MetroDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = user?.Id;

            // Materialize to List<T> and optionally manipulate in-memory (rubric List<T> usage)
            List<Issue> myReports = await _db.Issues
                .Where(i => i.CreatedByUserId == userId)
                .OrderByDescending(i => i.Id)
                .ToListAsync();

            // Example in-memory staging/filtering step (still List<T>)
            var displayReports = new List<Issue>(myReports); // could filter/search/paginate here

            List<Announcement> announcements = await _db.Announcements
                .OrderByDescending(a => a.Id)
                .Take(5)
                .ToListAsync();

            var vm = new ClientDashboardVm
            {
                DisplayName = user?.DisplayName ?? user?.Email ?? "Client",
                MyReports = displayReports,
                Announcements = announcements
            };

            return View(vm);
        }
    }
}

