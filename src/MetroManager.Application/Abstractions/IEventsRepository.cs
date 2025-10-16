using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MetroManager.Domain.Entities;

namespace MetroManager.Application.Abstractions
{
    public interface IEventRepository
    {
        Task<Event?> GetAsync(int id);
        Task<List<Event>> GetAllAsync();
        Task<List<Event>> GetFutureAsync(DateTime utcNow);
        Task<List<string>> GetAllCategoriesAsync();
        Task AddAsync(Event entity);
        Task UpdateAsync(Event entity);
        Task DeleteAsync(int id);
        Task UpsertAsync(Event entity) =>
            (entity.Id == 0) ? AddAsync(entity) : UpdateAsync(entity);
    }
}
