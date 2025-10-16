using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MetroManager.Infrastructure.Data;
using MetroManager.Domain.Entities;
using MetroManager.Domain.Enums;

namespace MetroManager.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly MetroDbContext _db;
        private const int PageSize = 50;

        public AdminController(MetroDbContext db) => _db = db;

        // ----- Landing dashboard -----
        // /Admin or /Admin/Index
        [HttpGet("")]
        [HttpGet("Admin")]
        [HttpGet("Admin/Index")]
        public async Task<IActionResult> Index()
        {
            var vm = new
            {
                Tickets = await _db.Issues.CountAsync(),
                // Remove the “Open” count to avoid referencing a missing enum member.
                Announcements = await _db.Announcements.CountAsync(),
                Events = await _db.Events.CountAsync()
            };

            return View(vm);
        }


        // ----- Ticket list with filter/search/paging -----
        // /Admin/Tickets
        [HttpGet("Admin/Tickets")]
        public async Task<IActionResult> Tickets(int? status, string? search, int page = 1)
        {
            if (page < 1) page = 1;

            IQueryable<Issue> q = _db.Issues.AsNoTracking();

            if (status.HasValue)
            {
                var st = (IssueStatus)status.Value;
                q = q.Where(i => i.Status == st);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(i =>
                    i.PublicId.Contains(s) ||
                    i.Category.Contains(s) ||
                    (i.Subcategory != null && i.Subcategory.Contains(s)) ||
                    i.LocationText.Contains(s) ||
                    i.Description.Contains(s));
            }

            q = q.OrderByDescending(i => i.Id);

            var total = await q.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page > totalPages) page = totalPages;

            var items = await q.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

            ViewBag.FilterStatus = status;
            ViewBag.Search = search ?? "";
            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalCount = total;
            ViewBag.PageSize = PageSize;

            return View(items);   // your existing Tickets view expects List<Issue>
        }

        // ----- Single-row actions -----

        [ValidateAntiForgeryToken]
        [HttpPost("Admin/Tickets/UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(int id, int status, string? notes)
        {
            var issue = await _db.Issues.FirstOrDefaultAsync(i => i.Id == id);
            if (issue == null) return NotFound();

            issue.Status = (IssueStatus)status;
            if (!string.IsNullOrWhiteSpace(notes)) issue.AdminNotes = notes.Trim();
            issue.UpdatedUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(new { ok = true });
        }

        [ValidateAntiForgeryToken]
        [HttpPost("Admin/Tickets/Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var issue = await _db.Issues.FirstOrDefaultAsync(i => i.Id == id);
            if (issue == null) return NotFound();

            _db.Issues.Remove(issue);
            await _db.SaveChangesAsync();

            TempData["Ok"] = "Ticket deleted.";
            return RedirectToAction(nameof(Tickets));
        }

        // ----- Bulk actions -----

        [ValidateAntiForgeryToken]
        [HttpPost("Admin/Tickets/BulkStatus")]
        public async Task<IActionResult> BulkStatus(string ids, int status, string? notes)
        {
            var idList = (ids ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out var v) ? v : (int?)null)
                .Where(v => v.HasValue).Select(v => v!.Value).ToList();

            if (idList.Count == 0)
            {
                TempData["Err"] = "No tickets selected.";
                return RedirectToAction(nameof(Tickets));
            }

            var st = (IssueStatus)status;
            var rows = await _db.Issues.Where(i => idList.Contains(i.Id)).ToListAsync();

            foreach (var i in rows)
            {
                i.Status = st;
                if (!string.IsNullOrWhiteSpace(notes)) i.AdminNotes = notes.Trim();
                i.UpdatedUtc = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            TempData["Ok"] = $"Updated {rows.Count} ticket(s).";
            return RedirectToAction(nameof(Tickets));
        }

        [ValidateAntiForgeryToken]
        [HttpPost("Admin/Tickets/BulkDelete")]
        public async Task<IActionResult> BulkDelete(string ids)
        {
            var idList = (ids ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x, out var v) ? v : (int?)null)
                .Where(v => v.HasValue).Select(v => v!.Value).ToList();

            if (idList.Count == 0)
            {
                TempData["Err"] = "No tickets selected.";
                return RedirectToAction(nameof(Tickets));
            }

            var rows = await _db.Issues.Where(i => idList.Contains(i.Id)).ToListAsync();
            _db.Issues.RemoveRange(rows);
            await _db.SaveChangesAsync();

            TempData["Ok"] = $"Deleted {rows.Count} ticket(s).";
            return RedirectToAction(nameof(Tickets));
        }

        // ----- Simple lists for events/announcements management -----

        [HttpGet("Admin/Events")]
        public async Task<IActionResult> Events()
        {
            var items = await _db.Events.AsNoTracking().OrderBy(e => e.StartsOn).ToListAsync();
            return View(items);
        }

        [HttpGet("Admin/Announcements")]
        public async Task<IActionResult> Announcements()
        {
            var items = await _db.Announcements.AsNoTracking().OrderByDescending(a => a.PublishedUtc).ToListAsync();
            return View(items);
        }
    }
}
