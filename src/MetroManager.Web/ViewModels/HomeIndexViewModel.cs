using System.Collections.Generic;
using MetroManager.Domain.Entities;

namespace MetroManager.Web.ViewModels
{
    public class HomeIndexViewModel
    {
        public List<Announcement> Announcements { get; set; } = new();
        public List<Event> UpcomingEvents { get; set; } = new();
    }
}


