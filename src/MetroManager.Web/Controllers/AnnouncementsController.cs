using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetroManager.Application.Abstractions;
using MetroManager.Web.ViewModels;

namespace MetroManager.Web.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly IAnnouncementRepository _repo;
        public AnnouncementsController(IAnnouncementRepository repo) => _repo = repo;

        [HttpGet("/announcements")]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var items = await _repo.GetAllAsync();
            var vms = items.Select(AnnouncementVm.FromEntity).ToList();
            ViewBag.ShowRecommendations = User?.Identity?.IsAuthenticated == true;
            return View(vms);
        }
    }
}
