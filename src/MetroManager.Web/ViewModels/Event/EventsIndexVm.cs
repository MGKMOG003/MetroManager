using System.Collections.Generic;

namespace MetroManager.Web.ViewModels.Events
{
    public class EventsIndexVm
    {
        public FilterVm Filters { get; set; } = new();
        public List<string> AvailableCategories { get; set; } = new();
        public List<EventCardVm> Events { get; set; } = new();
        public List<EventCardVm> Recommendations { get; set; } = new();
        public bool ShowRecommendations { get; set; }
    }
}
