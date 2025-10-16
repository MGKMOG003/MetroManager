using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MetroManager.Web.ViewModels
{
    public class IssueCreateVm
    {
        [Required, StringLength(120)]
        public string Title { get; set; } = string.Empty;

        [Required, StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string LocationText { get; set; } = string.Empty;

        [Url]
        public string? LocationLink { get; set; }

        [Required]
        public string Category { get; set; } = "Roads & Transport";

        public string? Subcategory { get; set; }
        public string? OtherCategory { get; set; }

        [Display(Name = "I confirm I did not include any personal information (POPIA)")]
        [Range(typeof(bool), "true", "true", ErrorMessage = "You must confirm no personal information is included.")]
        public bool ConsentPopia { get; set; }

        public IFormFileCollection? Files { get; set; }

        // Dropdown sources (never null)
        public List<string> Categories { get; set; } = new();

        // Category → Subcategories
        public Dictionary<string, List<string>> CategorySubcategories { get; set; } = new();
    }
}
