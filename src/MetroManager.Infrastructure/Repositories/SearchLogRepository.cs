using System.Threading.Tasks;
using MetroManager.Application.Abstractions;
using MetroManager.Infrastructure.Data;

namespace MetroManager.Infrastructure.Repositories
{
    // POPIA-friendly stub. Extend with a SearchLog table later if desired.
    public class SearchLogRepository : ISearchLogRepository
    {
        public Task LogAsync(SearchQueryDto query) => Task.CompletedTask;
    }
}
