using System.Collections.Generic;
using System.Threading.Tasks;
using MetroManager.Domain.Entities;
using MetroManager.Domain.Enums;

namespace MetroManager.Application.Abstractions
{
    public interface IIssueRepository
    {
        Task<Issue?> GetAsync(int id);
        Task<Issue?> GetByPublicIdAsync(string publicId);
        Task<List<Issue>> GetAllAsync(IssueStatus? status = null, string? search = null);
        Task UpdateAsync(Issue issue);
        Task DeleteAsync(Issue issue);
    }
}
