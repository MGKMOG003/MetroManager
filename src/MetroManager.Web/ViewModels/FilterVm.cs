using System;

namespace MetroManager.Web.ViewModels
{
    public class FilterVm
    {
        public string[]? Categories { get; set; }
        public DateTimeOffset? From { get; set; }
        public DateTimeOffset? To { get; set; }
    }
}
