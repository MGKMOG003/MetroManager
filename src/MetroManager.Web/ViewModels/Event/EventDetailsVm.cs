using System;
using EventEntity = MetroManager.Domain.Entities.Event;

namespace MetroManager.Web.ViewModels.Events
{
    public class EventDetailsVm
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public DateTime StartsOn { get; init; }
        public DateTime? EndsOn { get; init; }
        public string? Venue { get; init; }
        public string? City { get; init; }
        public string? LocationAddress { get; init; }
        public double? Latitude { get; init; }
        public double? Longitude { get; init; }
        public decimal? EntryPrice { get; init; }
        public string? AgeRestriction { get; init; }
        public string? MediaUrl { get; init; }
        public string? Url { get; init; }
        public string? TagsCsv { get; init; }

        public static EventDetailsVm FromEntity(EventEntity e) => new()
        {
            Id = e.Id,
            Title = e.Title,
            Description = e.Description,
            Category = e.Category,
            StartsOn = e.StartsOn,
            EndsOn = e.EndsOn,
            Venue = e.Venue,
            City = e.City,
            LocationAddress = e.LocationAddress,
            Latitude = e.Latitude,
            Longitude = e.Longitude,
            EntryPrice = e.EntryPrice,
            AgeRestriction = e.AgeRestriction,
            MediaUrl = e.MediaUrl,
            Url = e.Url,
            TagsCsv = e.TagsCsv
        };
    }
}
