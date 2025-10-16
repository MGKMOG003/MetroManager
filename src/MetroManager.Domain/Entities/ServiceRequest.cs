using System;
using System.Collections.Generic;

namespace MetroManager.Domain.Entities
{
    public class ServiceRequest
    {
        public int Id { get; set; }
        public string RequestCode { get; set; } = default!; // unique reference for the request
        public string CurrentStatus { get; set; } = default!;

        public int IssueId { get; set; }
        public Issue Issue { get; set; } = default!;

        public List<StatusHistory> History { get; set; } = new();
    }
}
