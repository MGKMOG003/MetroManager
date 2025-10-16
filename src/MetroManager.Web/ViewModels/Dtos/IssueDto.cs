namespace MetroManager.Web.ViewModels.Dtos
{
    public class IssueDto
    {
        public int Id { get; set; }
        public string PublicId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Subcategory { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Unassigned";
        public DateTime CreatedUtc { get; set; }
    }
}
