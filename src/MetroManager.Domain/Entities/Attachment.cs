using System;

namespace MetroManager.Domain.Entities
{
    public class Attachment
    {
        public int Id { get; set; }

        public int IssueId { get; set; }
        public Issue Issue { get; set; } = default!;

        public string FileName { get; set; } = default!;
        public string StoredPath { get; set; } = default!;
        public string ContentType { get; set; } = default!;
        public DateTime UploadedUtc { get; set; } = DateTime.UtcNow;
    }
}
