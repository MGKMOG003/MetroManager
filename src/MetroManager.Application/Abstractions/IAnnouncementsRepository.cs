using System.Collections.Generic;
using System.Threading.Tasks;
using MetroManager.Domain.Entities;

namespace MetroManager.Application.Abstractions
{
    public interface IAnnouncementRepository
    {
        Task<Announcement?> GetAsync(int id);
        Task<List<Announcement>> GetAllAsync();
        Task<List<Announcement>> GetRecentAsync(int take);
        Task AddAsync(Announcement entity);
        Task UpdateAsync(Announcement entity);
        Task DeleteAsync(int id);
        Task UpsertAsync(Announcement entity) =>
            (entity.Id == 0) ? AddAsync(entity) : UpdateAsync(entity);
    }
}
