using System.Collections.Generic;
using MetroManager.Domain.Entities;

namespace MetroManager.Web.ViewModels.Dashboard
{
    public class ClientDashboardVm
    {
        public string DisplayName { get; set; } = "Client";
        public List<Issue> MyReports { get; set; } = new();
        public List<Announcement> Announcements { get; set; } = new();
    }
}

