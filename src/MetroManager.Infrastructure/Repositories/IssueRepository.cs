using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MetroManager.Application.Abstractions;     // <-- now implements interface from Application
using MetroManager.Domain.Entities;
using MetroManager.Domain.Enums;
using MetroManager.Infrastructure.Data;

namespace MetroManager.Infrastructure.Repositories
{
    public class IssueRepository : IIssueRepository
    {
        private readonly MetroDbContext _db;
        public IssueRepository(MetroDbContext db) => _db = db;

        public Task<Issue?> GetAsync(int id) =>
            _db.Issues.Include(x => x.Attachments).FirstOrDefaultAsync(x => x.Id == id);

        public Task<Issue?> GetByPublicIdAsync(string publicId) =>
            _db.Issues.Include(x => x.Attachments).FirstOrDefaultAsync(x => x.PublicId == publicId);

        public async Task<List<Issue>> GetAllAsync(IssueStatus? status = null, string? search = null)
        {
            var q = _db.Issues.AsQueryable();
            if (status.HasValue) q = q.Where(x => x.Status == status);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                q = q.Where(x =>
                    x.PublicId.Contains(s) ||
                    x.Category.Contains(s) ||
                    (x.Subcategory ?? "").Contains(s) ||
                    x.LocationText.Contains(s) ||
                    x.Description.Contains(s));
            }
            return await q.OrderByDescending(x => x.CreatedUtc).ToListAsync();
        }

        public async Task UpdateAsync(Issue issue)
        {
            _db.Issues.Update(issue);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Issue issue)
        {
            _db.Issues.Remove(issue);
            await _db.SaveChangesAsync();
        }
    }
}
