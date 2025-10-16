using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetroManager.Application.Issues.Admin;
using MetroManager.Application.Issues.Admin.Models;
using MetroManager.Domain.Enums;
using MetroManager.Web.ViewModels.Admin.Issues;

namespace MetroManager.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    [Route("Admin/Issues")]
    public class IssuesController : Controller
    {
        private const int PageSize = 50;
        private readonly IIssueAdminService _svc;
        public IssuesController(IIssueAdminService svc) => _svc = svc;

        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] int? status, [FromQuery] string? search, [FromQuery] int page = 1)
        {
            if (page < 1) page = 1;

            var list = await _svc.ListAsync(status, search);
            var total = list.Count;
            var totalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
            if (page > totalPages) page = totalPages;

            var items = list.Skip((page - 1) * PageSize).Take(PageSize).ToList();

            var vm = new AdminIssueIndexVm
            {
                FilterStatus = status,
                Search = search,
                Items = items.Select(i => new AdminIssueListItemVm
                {
                    Id = i.Id,
                    PublicId = i.PublicId,
                    Category = i.Category,
                    Subcategory = i.Subcategory,
                    LocationText = i.LocationText,
                    DescriptionShort = i.DescriptionShort,
                    Status = (IssueStatus)i.Status,
                    CreatedUtc = i.CreatedUtc
                }).ToList(),
                Page = page,
                TotalPages = totalPages,
                TotalCount = total,
                PageSize = PageSize
            };

            return View(vm);
        }

        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _svc.GetForEditAsync(id);
            if (dto == null) return NotFound();

            var vm = new AdminIssueEditVm
            {
                Id = dto.Id,
                PublicId = dto.PublicId,
                Category = dto.Category,
                Subcategory = dto.Subcategory,
                LocationText = dto.LocationText,
                LocationLink = dto.LocationLink,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Description = dto.Description,
                Status = (IssueStatus)dto.Status,
                AdminNotes = dto.AdminNotes
            };

            return View(vm);
        }

        [ValidateAntiForgeryToken]
        [HttpPost("Edit/{id:int}")]
        public async Task<IActionResult> Edit(AdminIssueEditVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var dto = new AdminIssueEditDto
            {
                Id = vm.Id,
                PublicId = vm.PublicId,
                Category = vm.Category,
                Subcategory = vm.Subcategory,
                LocationText = vm.LocationText,
                LocationLink = vm.LocationLink,
                Latitude = vm.Latitude,
                Longitude = vm.Longitude,
                Description = vm.Description,
                Status = (int)vm.Status,
                AdminNotes = vm.AdminNotes
            };

            await _svc.UpdateAsync(dto);
            TempData["Ok"] = $"Issue {vm.PublicId} updated.";
            return RedirectToAction(nameof(Index));
        }

        [ValidateAntiForgeryToken]
        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus([FromForm] int id, [FromForm] int status, [FromForm] string? notes)
        {
            await _svc.UpdateStatusAsync(id, status, notes);
            return Ok(new { ok = true });
        }

        // NEW: bulk status
        [ValidateAntiForgeryToken]
        [HttpPost("BulkStatus")]
        public async Task<IActionResult> BulkStatus([FromForm] string ids, [FromForm] int status, [FromForm] string? notes)
        {
            var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            if (idList.Count == 0) return BadRequest();
            await _svc.UpdateStatusBulkAsync(idList, status, notes);
            TempData["Ok"] = $"Updated {idList.Count} ticket(s).";
            return RedirectToAction(nameof(Index), new { status = (int?)null, search = (string?)null });
        }

        [ValidateAntiForgeryToken]
        [HttpPost("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _svc.DeleteAsync(id);
            TempData["Ok"] = "Issue deleted.";
            return RedirectToAction(nameof(Index));
        }

        // NEW: bulk delete
        [ValidateAntiForgeryToken]
        [HttpPost("BulkDelete")]
        public async Task<IActionResult> BulkDelete([FromForm] string ids)
        {
            var idList = ids.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
            if (idList.Count == 0) return BadRequest();
            await _svc.DeleteManyAsync(idList);
            TempData["Ok"] = $"Deleted {idList.Count} ticket(s).";
            return RedirectToAction(nameof(Index));
        }
    }
}
