using System.Collections.Generic;
using System.Threading.Tasks;
using MetroManager.Application.Issues.Admin.Models;

namespace MetroManager.Application.Issues.Admin
{
    public interface IIssueAdminService
    {
        Task<List<AdminIssueListItemDto>> ListAsync(int? status, string? search);
        Task<AdminIssueEditDto?> GetForEditAsync(int id);
        Task UpdateAsync(AdminIssueEditDto dto);
        Task UpdateStatusAsync(int id, int status, string? adminNotes);

        // NEW
        Task UpdateStatusBulkAsync(IEnumerable<int> ids, int status, string? adminNotes);
        Task DeleteAsync(int id);
        Task DeleteManyAsync(IEnumerable<int> ids);
    }
}

