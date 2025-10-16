using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using MetroManager.Domain.Entities;

namespace MetroManager.Application.Services.Events
{
    /// <summary>
    /// Thread-safe in-memory indices + signals to support fast search and recommendations.
    /// Uses: SortedDictionary (by date), Dictionary/HashSet, Stack, Queue, PriorityQueue.
    /// </summary>
    public sealed class EventsIndex
    {
        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.SupportsRecursion);

        private SortedDictionary<DateOnly, List<Event>> _byDate = new();
        private Dictionary<int, Event> _byId = new();
        private Dictionary<string, HashSet<int>> _idsByCategory = new(StringComparer.OrdinalIgnoreCase);
        private HashSet<string> _allCategories = new(StringComparer.OrdinalIgnoreCase);
        private PriorityQueue<Event, DateTime> _upcoming = new();

        private readonly Queue<(IReadOnlyCollection<string> cats, DateTime ts)> _recentQueries = new();
        private readonly int _maxQueries = 64;

        private readonly Stack<int> _recentlyViewed = new();
        private readonly int _maxViewed = 16;

        public void Rebuild(IEnumerable<Event> events)
        {
            _lock.EnterWriteLock();
            try
            {
                _byDate = new();
                _byId = new();
                _idsByCategory = new(StringComparer.OrdinalIgnoreCase);
                _allCategories = new(StringComparer.OrdinalIgnoreCase);
                _upcoming = new();

                foreach (var e in events)
                {
                    _byId[e.Id] = e;

                    var d = DateOnly.FromDateTime(e.StartsOn);
                    if (!_byDate.TryGetValue(d, out var list))
                    {
                        list = new List<Event>();
                        _byDate[d] = list;
                    }
                    list.Add(e);

                    if (!string.IsNullOrWhiteSpace(e.Category))
                    {
                        if (!_idsByCategory.TryGetValue(e.Category, out var set))
                            _idsByCategory[e.Category] = set = new HashSet<int>();
                        set.Add(e.Id);
                        _allCategories.Add(e.Category);
                    }

                    if (e.StartsOn >= DateTime.UtcNow)
                        _upcoming.Enqueue(e, e.StartsOn);
                }

                foreach (var kv in _byDate)
                    kv.Value.Sort((a, b) => a.StartsOn.CompareTo(b.StartsOn));
            }
            finally { _lock.ExitWriteLock(); }
        }

        public IReadOnlyCollection<string> Categories
        {
            get { _lock.EnterReadLock(); try { return _allCategories.ToArray(); } finally { _lock.ExitReadLock(); } }
        }

        public void EnqueueSearch(IReadOnlyCollection<string> categories)
        {
            _lock.EnterWriteLock();
            try
            {
                _recentQueries.Enqueue((categories, DateTime.UtcNow));
                while (_recentQueries.Count > _maxQueries) _recentQueries.Dequeue();
            }
            finally { _lock.ExitWriteLock(); }
        }

        public void PushRecentlyViewed(int eventId)
        {
            _lock.EnterWriteLock();
            try
            {
                _recentlyViewed.Push(eventId);
                while (_recentlyViewed.Count > _maxViewed) _recentlyViewed.Pop();
            }
            finally { _lock.ExitWriteLock(); }
        }

        public IEnumerable<Event> PeekUpcoming(int take)
        {
            _lock.EnterReadLock();
            try
            {
                var tmp = new PriorityQueue<Event, DateTime>(_upcoming.UnorderedItems);
                var outList = new List<Event>(Math.Min(take, tmp.Count));
                while (outList.Count < take && tmp.TryDequeue(out var e, out _))
                    outList.Add(e);
                return outList;
            }
            finally { _lock.ExitReadLock(); }
        }

        public IEnumerable<Event> Search(IEnumerable<string> categories, DateOnly? from, DateOnly? to)
        {
            _lock.EnterReadLock();
            try
            {
                IEnumerable<int> candidateIds;
                var cats = (categories ?? Enumerable.Empty<string>())
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (cats.Count > 0)
                {
                    HashSet<int>? acc = null;
                    foreach (var c in cats)
                    {
                        if (!_idsByCategory.TryGetValue(c, out var set))
                            return Enumerable.Empty<Event>();
                        acc = acc is null ? new HashSet<int>(set) : new HashSet<int>(acc.Intersect(set));
                        if (acc.Count == 0) return Enumerable.Empty<Event>();
                    }
                    candidateIds = acc!;
                }
                else
                {
                    candidateIds = _byId.Keys;
                }

                IEnumerable<Event> filtered = EnumerateRange(from, to).Where(e => candidateIds.Contains(e.Id));
                if (from is null && to is null && cats.Count == 0)
                    filtered = _byId.Values;
                return filtered;
            }
            finally { _lock.ExitReadLock(); }
        }

        private IEnumerable<Event> EnumerateRange(DateOnly? from, DateOnly? to)
        {
            if (_byDate.Count == 0) return Enumerable.Empty<Event>();
            if (from is null && to is null) return _byId.Values;

            var start = from ?? _byDate.Keys.First();
            var end = to ?? _byDate.Keys.Last();
            var keys = _byDate.Keys.Where(k => k >= start && k <= end);
            return keys.SelectMany(k => _byDate[k]);
        }

        public IReadOnlyDictionary<string, int> CategoryFrequency(int maxWindow = 30)
        {
            _lock.EnterReadLock();
            try
            {
                var items = _recentQueries.Reverse().Take(maxWindow).ToList();
                var freq = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                int weight = items.Count;
                foreach (var (cats, _) in items)
                {
                    foreach (var c in cats)
                    {
                        if (!freq.TryGetValue(c, out var n)) n = 0;
                        freq[c] = n + Math.Max(1, weight);
                    }
                    weight--;
                }
                return freq;
            }
            finally { _lock.ExitReadLock(); }
        }

        public IReadOnlyCollection<int> RecentlyViewed(int take) =>
            _recentlyViewed.Take(take).ToArray();
    }
}
