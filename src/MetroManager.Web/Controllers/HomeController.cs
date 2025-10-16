using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MetroManager.Web.Models;
using MetroManager.Infrastructure.Data;
using MetroManager.Web.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace MetroManager.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MetroDbContext _db;

        public HomeController(ILogger<HomeController> logger, MetroDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var announcements = await _db.Announcements
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.PublishedUtc)
                .Take(6)
                .ToListAsync();

            var upcoming = await _db.Events
                .Where(e => e.StartsOn >= DateTime.UtcNow)
                .OrderBy(e => e.StartsOn)
                .Take(6)
                .ToListAsync();

            var vm = new HomeIndexViewModel
            {
                Announcements = announcements,
                UpcomingEvents = upcoming
            };

            return View(vm);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
