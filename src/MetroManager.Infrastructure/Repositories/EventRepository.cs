using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetroManager.Application.Abstractions;
using MetroManager.Domain.Entities;
using MetroManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MetroManager.Infrastructure.Repositories
{
    public class EventRepository : IEventRepository
    {
        private readonly MetroDbContext _db;
        public EventRepository(MetroDbContext db) => _db = db;

        public Task<Event?> GetAsync(int id) =>
            _db.Set<Event>().FirstOrDefaultAsync(x => x.Id == id)!;

        public Task<List<Event>> GetAllAsync() =>
            _db.Set<Event>().OrderBy(e => e.StartsOn).ToListAsync();

        public Task<List<Event>> GetFutureAsync(DateTime utcNow) =>
            _db.Set<Event>()
               .Where(e => e.StartsOn >= utcNow)
               .OrderBy(e => e.StartsOn)
               .ToListAsync();

        // 👇 NO angle bracket here. It must be exactly like this:
        public async Task<List<string>> GetAllCategoriesAsync()
        {
            return await _db.Set<Event>()
                .Select(e => e.Category)
                .Where(c => c != null && c != "")
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task AddAsync(Event entity)
        {
            _db.Set<Event>().Add(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Event entity)
        {
            _db.Set<Event>().Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var e = await _db.Set<Event>().FindAsync(id);
            if (e is null) return;
            _db.Set<Event>().Remove(e);
            await _db.SaveChangesAsync();
        }
    }
}
