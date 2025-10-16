namespace MetroManager.Web.ViewModels.Admin.Issues
{
    public class AdminIssueListItemDto
    {
        public int Id { get; set; }
        public string PublicId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string LocationText { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
    }
}
