using System;

namespace MetroManager.Domain.Entities
{
    public class StatusHistory
    {
        public int Id { get; set; }
        public int ServiceRequestId { get; set; }
        public string Status { get; set; } = default!;
        public string? Notes { get; set; }
        public DateTime ChangedUtc { get; set; } = DateTime.UtcNow;

        public ServiceRequest ServiceRequest { get; set; } = default!;
    }
}
