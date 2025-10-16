using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetroManager.Application.Abstractions;
using MetroManager.Application.Issues.Admin.Models;
using MetroManager.Domain.Enums;

namespace MetroManager.Application.Issues.Admin
{
    public class IssueAdminService : IIssueAdminService
    {
        private readonly IIssueRepository _repo;
        public IssueAdminService(IIssueRepository repo) => _repo = repo;

        public async Task<List<AdminIssueListItemDto>> ListAsync(int? status, string? search)
        {
            IssueStatus? st = status.HasValue ? (IssueStatus?)status.Value : null;
            var items = await _repo.GetAllAsync(st, search);
            return items.Select(i => new AdminIssueListItemDto
            {
                Id = i.Id,
                PublicId = i.PublicId,
                Category = i.Category,
                Subcategory = i.Subcategory,
                LocationText = i.LocationText,
                Status = (int)i.Status,
                CreatedUtc = i.CreatedUtc,
                DescriptionShort = i.Description.Length > 120 ? i.Description[..120] + "…" : i.Description
            }).ToList();
        }

        public async Task<AdminIssueEditDto?> GetForEditAsync(int id)
        {
            var i = await _repo.GetAsync(id);
            if (i == null) return null;

            return new AdminIssueEditDto
            {
                Id = i.Id,
                PublicId = i.PublicId,
                Category = i.Category,
                Subcategory = i.Subcategory,
                LocationText = i.LocationText,
                LocationLink = i.LocationLink,
                Latitude = i.Latitude,
                Longitude = i.Longitude,
                Description = i.Description,
                Status = (int)i.Status,
                AdminNotes = i.AdminNotes
            };
        }

        public async Task UpdateAsync(AdminIssueEditDto dto)
        {
            var i = await _repo.GetAsync(dto.Id) ?? throw new InvalidOperationException("Issue not found.");
            i.Category = dto.Category.Trim();
            i.Subcategory = string.IsNullOrWhiteSpace(dto.Subcategory) ? null : dto.Subcategory.Trim();
            i.LocationText = dto.LocationText.Trim();
            i.LocationLink = string.IsNullOrWhiteSpace(dto.LocationLink) ? null : dto.LocationLink.Trim();
            i.Latitude = dto.Latitude;
            i.Longitude = dto.Longitude;
            i.Description = dto.Description.Trim();
            i.Status = (IssueStatus)dto.Status;
            i.AdminNotes = string.IsNullOrWhiteSpace(dto.AdminNotes) ? null : dto.AdminNotes.Trim();
            i.UpdatedUtc = DateTime.UtcNow;
            await _repo.UpdateAsync(i);
        }

        public async Task UpdateStatusAsync(int id, int status, string? adminNotes)
        {
            var i = await _repo.GetAsync(id) ?? throw new InvalidOperationException("Issue not found.");
            i.Status = (IssueStatus)status;
            if (!string.IsNullOrWhiteSpace(adminNotes)) i.AdminNotes = adminNotes.Trim();
            i.UpdatedUtc = DateTime.UtcNow;
            await _repo.UpdateAsync(i);
        }

        // NEW: bulk status
        public async Task UpdateStatusBulkAsync(IEnumerable<int> ids, int status, string? adminNotes)
        {
            foreach (var id in ids)
            {
                var i = await _repo.GetAsync(id);
                if (i == null) continue;
                i.Status = (IssueStatus)status;
                if (!string.IsNullOrWhiteSpace(adminNotes)) i.AdminNotes = adminNotes.Trim();
                i.UpdatedUtc = DateTime.UtcNow;
                await _repo.UpdateAsync(i);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var i = await _repo.GetAsync(id) ?? throw new InvalidOperationException("Issue not found.");
            await _repo.DeleteAsync(i);
        }

        // NEW: bulk delete
        public async Task DeleteManyAsync(IEnumerable<int> ids)
        {
            foreach (var id in ids)
            {
                var i = await _repo.GetAsync(id);
                if (i != null)
                    await _repo.DeleteAsync(i);
            }
        }
    }
}
