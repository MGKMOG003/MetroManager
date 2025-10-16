using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MetroManager.Domain.Entities;

namespace MetroManager.Application.Services.Events
{
    public sealed class RecommendationService
    {
        private readonly EventsIndex _index;

        public RecommendationService(EventsIndex index) => _index = index;

        public Task<IReadOnlyList<Event>> RecommendAsync(int take = 5)
        {
            var freq = _index.CategoryFrequency(30)
                             .OrderByDescending(kv => kv.Value)
                             .Take(3)
                             .Select(kv => kv.Key)
                             .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var recentlyViewed = _index.RecentlyViewed(10).ToHashSet();

            var pool = _index.PeekUpcoming(take * 6).ToList();
            var scored = new List<(double score, Event e)>();

            foreach (var e in pool)
            {
                if (recentlyViewed.Contains(e.Id)) continue;

                // Category from entity (single) + tags if present
                var tags = (e.TagsCsv ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var cats = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { e.Category };
                foreach (var t in tags) cats.Add(t);

                double jacc = Jaccard(cats, freq);
                double temporal = e.StartsOn <= DateTime.UtcNow ? 0 : 1.0 / (1.0 + (e.StartsOn - DateTime.UtcNow).TotalHours / 24.0);

                double score = 0.65 * jacc + 0.35 * temporal;
                if (score > 0) scored.Add((score, e));
            }

            var top = scored.OrderByDescending(s => s.score).Take(take).Select(s => s.e).ToList();
            return Task.FromResult<IReadOnlyList<Event>>(top);
        }

        private static double Jaccard(HashSet<string> a, HashSet<string> b)
        {
            if (a.Count == 0 || b.Count == 0) return 0;
            int inter = a.Intersect(b).Count();
            int union = a.Union(b).Count();
            return union == 0 ? 0 : (double)inter / union;
        }
    }
}
