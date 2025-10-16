using System;

namespace MetroManager.Domain.Entities
{
    public class Announcement
    {
        public int Id { get; set; }

        public string Title { get; set; } = default!;
        public string? Summary { get; set; }              // short text for cards
        public string? Body { get; set; }                 // full content
        public string Category { get; set; } = "General"; // Outage, Event, Safety, etc.
        public bool IsPinned { get; set; }                // float to top
        public DateTime PublishedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresUtc { get; set; }

        // NEW (Part 2 requirements)
        public string? Link { get; set; }                 // external URL
        public string? MediaUrl { get; set; }             // image/file for tile
    }
}
