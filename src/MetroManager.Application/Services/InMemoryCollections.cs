using System.Collections.Generic;
using MetroManager.Domain.Entities;

namespace MetroManager.Application.Services
{
    // Lightweight user snapshot for temp lists (keeps Application layer clean)
    public class UserLite
    {
        public string? Id { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
    }

    /// <summary>
    /// Temporary in-memory collections to satisfy List&lt;T&gt; usage.
    /// </summary>
    public class InMemoryCollections
    {
        private readonly List<Issue> _recentIssues = new();
        private readonly List<UserLite> _recentUsers = new();

        public IReadOnlyList<Issue> RecentIssues => _recentIssues.AsReadOnly();
        public IReadOnlyList<UserLite> RecentUsers => _recentUsers.AsReadOnly();

        public void AddIssueTemp(Issue issue)
        {
            _recentIssues.Add(issue);
            if (_recentIssues.Count > 25) _recentIssues.RemoveAt(0);
        }

        public void AddUserTemp(UserLite user)
        {
            _recentUsers.Add(user);
            if (_recentUsers.Count > 25) _recentUsers.RemoveAt(0);
        }

        public List<Issue> FilterIssuesByCategory(string category)
        {
            var list = new List<Issue>();
            foreach (var it in _recentIssues)
                if (it.Category == category) list.Add(it);
            return list;
        }
    }
}
