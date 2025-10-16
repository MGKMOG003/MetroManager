namespace MetroManager.Application.Issues.Admin.Models
{
    public class AdminIssueListItemDto
    {
        public int Id { get; set; }
        public string PublicId { get; set; } = default!;
        public string Category { get; set; } = default!;
        public string? Subcategory { get; set; }
        public string LocationText { get; set; } = default!;
        public string DescriptionShort { get; set; } = default!;
        public int Status { get; set; } // map from enum int
        public DateTime CreatedUtc { get; set; }
    }
}
