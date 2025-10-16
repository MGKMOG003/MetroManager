using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetroManager.Application.Abstractions;
using MetroManager.Domain.Entities;
using MetroManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetroManager.Infrastructure.Repositories
{
    public class AnnouncementRepository : IAnnouncementRepository
    {
        private readonly MetroDbContext _db;
        public AnnouncementRepository(MetroDbContext db) => _db = db;

        public Task<Announcement?> GetAsync(int id) =>
            _db.Set<Announcement>().FirstOrDefaultAsync(a => a.Id == id)!;

        public Task<List<Announcement>> GetAllAsync() =>
            _db.Set<Announcement>()
               .OrderByDescending(a => a.IsPinned)
               .ThenByDescending(a => a.PublishedUtc)
               .ToListAsync();

        public Task<List<Announcement>> GetRecentAsync(int take) =>
            _db.Set<Announcement>()
               .OrderByDescending(a => a.IsPinned)
               .ThenByDescending(a => a.PublishedUtc)
               .Take(take)
               .ToListAsync();

        public async Task AddAsync(Announcement entity)
        {
            _db.Set<Announcement>().Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Announcement entity)
        {
            _db.Set<Announcement>().Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var a = await _db.Set<Announcement>().FindAsync(id);
            if (a is null) return;
            _db.Set<Announcement>().Remove(a);
            await _db.SaveChangesAsync();
        }
    }
}
