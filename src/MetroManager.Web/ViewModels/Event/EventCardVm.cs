using System;
using EventEntity = MetroManager.Domain.Entities.Event;

namespace MetroManager.Web.ViewModels.Events
{
    public class EventCardVm
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Venue { get; init; }
        public string? City { get; init; }
        public string Category { get; init; } = string.Empty;
        public DateTime StartsOn { get; init; }
        public DateTime? EndsOn { get; init; }
        public decimal? EntryPrice { get; init; }
        public string? AgeRestriction { get; init; }
        public string? MediaUrl { get; init; }

        public static EventCardVm FromEntity(EventEntity e) => new()
        {
            Id = e.Id,
            Title = e.Title,
            Venue = e.Venue,
            City = e.City,
            Category = e.Category,
            StartsOn = e.StartsOn,
            EndsOn = e.EndsOn,
            EntryPrice = e.EntryPrice,
            AgeRestriction = e.AgeRestriction,
            MediaUrl = e.MediaUrl
        };
    }
}
