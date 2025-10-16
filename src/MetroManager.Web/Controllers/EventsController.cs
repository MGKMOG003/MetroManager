using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetroManager.Application.Abstractions;
using MetroManager.Application.Services.Events;
using MetroManager.Web.Services;
using MetroManager.Web.ViewModels;
using MetroManager.Web.ViewModels.Events;



namespace MetroManager.Web.Controllers
{
    public class EventsController : Controller
    {
        private readonly SearchService _search;
        private readonly RecommendationService _recs;
        private readonly IEventRepository _eventsRepo;
        private readonly ClientFingerprint _fp;

        public EventsController(SearchService search, RecommendationService recs, IEventRepository eventsRepo, ClientFingerprint fp)
        {
            _search = search;
            _recs = recs;
            _eventsRepo = eventsRepo;
            _fp = fp;
        }

        [HttpGet("/events")]
        [AllowAnonymous]
        public async Task<IActionResult> Index([FromQuery] FilterVm filter)
        {
            var fingerprint = _fp.EnsureFingerprint(HttpContext);

            var categories = (filter.Categories ?? Array.Empty<string>())
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            DateTime? fromUtc = filter.From?.UtcDateTime;
            DateTime? toUtc = filter.To?.UtcDateTime;

            string? userId = User?.Identity?.IsAuthenticated == true
                ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                : null;

            var results = await _search.SearchAsync(categories, fromUtc, toUtc, fingerprint, userId);

            var vms = results.Select(EventCardVm.FromEntity).ToList();
            var allCats = await _eventsRepo.GetAllCategoriesAsync();

            var recVms = (User?.Identity?.IsAuthenticated == true)
                ? (await _recs.RecommendAsync(5)).Select(EventCardVm.FromEntity).ToList()
                : new();

            var model = new EventsIndexVm
            {
                Filters = new FilterVm { Categories = categories, From = filter.From, To = filter.To },
                AvailableCategories = allCats,
                Events = vms,
                Recommendations = recVms,
                ShowRecommendations = User?.Identity?.IsAuthenticated == true
            };

            return View(model);
        }

        [HttpGet("/events/{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id, [FromServices] EventsIndex index)
        {
            var evt = await _eventsRepo.GetAsync(id);
            if (evt is null) return NotFound();

            if (User?.Identity?.IsAuthenticated == true)
                index.PushRecentlyViewed(evt.Id);

            var vm = EventDetailsVm.FromEntity(evt);
            return View(vm);
        }
    }
}
