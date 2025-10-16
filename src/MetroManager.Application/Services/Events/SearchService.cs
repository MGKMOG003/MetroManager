using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetroManager.Application.Abstractions;
using MetroManager.Domain.Entities;

namespace MetroManager.Application.Services.Events
{
    public sealed class SearchService
    {
        private readonly IEventRepository _events;
        private readonly ISearchLogRepository _logs;
        private readonly EventsIndex _index;

        public SearchService(IEventRepository events, ISearchLogRepository logs, EventsIndex index)
        {
            _events = events;
            _logs = logs;
            _index = index;
        }

        public async Task<IReadOnlyList<Event>> SearchAsync(
            IReadOnlyCollection<string> categories,
            DateTime? fromUtc,
            DateTime? toUtc,
            string clientFingerprint,
            string? userId = null)
        {
            if (!_index.Categories.Any())
            {
                var all = await _events.GetFutureAsync(DateTime.UtcNow);
                _index.Rebuild(all);
            }

            await _logs.LogAsync(new SearchQueryDto(
                Categories: categories,
                FromUtc: fromUtc,
                ToUtc: toUtc,
                ClientFingerprint: clientFingerprint,
                OccurredUtc: DateTime.UtcNow,
                UserId: userId));

            _index.EnqueueSearch(categories);

            DateOnly? from = fromUtc.HasValue ? DateOnly.FromDateTime(fromUtc.Value) : (DateOnly?)null;
            DateOnly? to = toUtc.HasValue ? DateOnly.FromDateTime(toUtc.Value) : (DateOnly?)null;

            return _index.Search(categories, from, to).OrderBy(e => e.StartsOn).ToList();
        }
    }
}
