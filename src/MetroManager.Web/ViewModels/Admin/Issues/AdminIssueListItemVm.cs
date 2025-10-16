using System;
using MetroManager.Domain.Enums;

namespace MetroManager.Web.ViewModels.Admin.Issues
{
    public class AdminIssueListItemVm
    {
        public int Id { get; set; }
        public string PublicId { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string? Subcategory { get; set; }
        public string LocationText { get; set; } = default!;
        public string DescriptionShort { get; set; } = default!;
        public IssueStatus Status { get; set; }
        public DateTime CreatedUtc { get; set; }
    }
}
