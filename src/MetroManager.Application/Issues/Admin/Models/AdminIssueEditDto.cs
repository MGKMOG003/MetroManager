using System.ComponentModel.DataAnnotations;

namespace MetroManager.Application.Issues.Admin.Models
{
    public class AdminIssueEditDto
    {
        public int Id { get; set; }

        [Display(Name = "Tracking ID")]
        public string PublicId { get; set; } = default!;

        [Required, StringLength(80)]
        public string Category { get; set; } = default!;

        [StringLength(80)]
        public string? Subcategory { get; set; }

        [Required, StringLength(200)]
        [Display(Name = "Location")]
        public string LocationText { get; set; } = default!;

        [Url, Display(Name = "Location Link")]
        public string? LocationLink { get; set; }

        [Range(-90, 90)]
        public double? Latitude { get; set; }

        [Range(-180, 180)]
        public double? Longitude { get; set; }

        [Required, StringLength(1000)]
        public string Description { get; set; } = default!;

        [Display(Name = "Status")]
        public int Status { get; set; } // enum as int

        [Display(Name = "Admin Notes")]
        [StringLength(2000)]
        public string? AdminNotes { get; set; }
    }
}
