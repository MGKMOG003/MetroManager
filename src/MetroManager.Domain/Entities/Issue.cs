using System;
using System.Collections.Generic;
using MetroManager.Domain.Enums;

namespace MetroManager.Domain.Entities
{
    public class Issue
    {
        public int Id { get; set; }

        // Public-friendly tracking id, e.g. "ISS-20250910...-123"
        public string PublicId { get; set; } = default!;

        // Classification
        public string Category { get; set; } = default!;
        public string? Subcategory { get; set; }

        // Location
        public string LocationText { get; set; } = default!;
        public string? LocationLink { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Description
        public string Description { get; set; } = default!;

        // Reporter
        public bool IsAnonymous { get; set; }
        public string? CreatedByUserId { get; set; }

        public DateTime CreatedUtc { get; set; }

        // NEW: Admin management
        public IssueStatus Status { get; set; } = IssueStatus.New;
        public string? AdminNotes { get; set; }
        public DateTime? UpdatedUtc { get; set; }

        // Relations
        public ServiceRequest? ServiceRequest { get; set; }
        public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    }
}
