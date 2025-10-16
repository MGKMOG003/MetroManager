using System;
using MetroManager.Domain.Entities;

namespace MetroManager.Web.ViewModels
{
    public class AnnouncementVm
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Summary { get; init; }
        public string? Body { get; init; }
        public string Category { get; init; } = "General";
        public bool IsPinned { get; init; }
        public DateTime PublishedUtc { get; init; }
        public string? Link { get; init; }
        public string? MediaUrl { get; init; }

        public static AnnouncementVm FromEntity(Announcement a) => new()
        {
            Id = a.Id,
            Title = a.Title,
            Summary = a.Summary,
            Body = a.Body,
            Category = a.Category,
            IsPinned = a.IsPinned,
            PublishedUtc = a.PublishedUtc,
            Link = a.Link,
            MediaUrl = a.MediaUrl
        };
    }
}

