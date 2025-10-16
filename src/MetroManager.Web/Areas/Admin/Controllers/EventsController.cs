using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetroManager.Application.Abstractions;
using MetroManager.Domain.Entities;

namespace MetroManager.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EventsController : Controller
    {
        private readonly IEventRepository _repo;
        public EventsController(IEventRepository repo) => _repo = repo;

        public async Task<IActionResult> Index()
        {
            var items = await _repo.GetAllAsync();
            return View(items);
        }

        public IActionResult Create() => View(new Event { StartsOn = System.DateTime.UtcNow });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event model)
        {
            if (!ModelState.IsValid) return View(model);
            await _repo.AddAsync(model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var item = await _repo.GetAsync(id);
            if (item is null) return NotFound();
            return View(item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Event model)
        {
            if (!ModelState.IsValid) return View(model);
            await _repo.UpdateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var item = await _repo.GetAsync(id);
            if (item is null) return NotFound();
            return View(item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _repo.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
