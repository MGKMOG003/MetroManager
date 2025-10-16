using System;

namespace MetroManager.Domain.Entities
{
    public class Event
    {
        public int Id { get; set; }

        // Required
        public string Title { get; set; } = default!;
        public string Category { get; set; } = default!;
        public DateTime StartsOn { get; set; }
        public string Description { get; set; } = default!;

        // Optional (existing)
        public DateTime? EndsOn { get; set; }
        public string? Venue { get; set; }
        public string? City { get; set; }
        public string? TagsCsv { get; set; }
        public string? Url { get; set; }

        // NEW (Part 2 requirements)
        public string? LocationAddress { get; set; }       // human-readable address for embedding
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public decimal? EntryPrice { get; set; }           // entry price if applicable
        public string? AgeRestriction { get; set; }        // "All ages", "13+", "18+", etc.
        public string? MediaUrl { get; set; }              // optional hero/media for card
    }
}
