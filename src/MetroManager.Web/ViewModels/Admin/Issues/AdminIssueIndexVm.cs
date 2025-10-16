using System.Collections.Generic;

namespace MetroManager.Web.ViewModels.Admin.Issues
{
    public class AdminIssueIndexVm
    {
        public int? FilterStatus { get; set; }
        public string? Search { get; set; }
        public List<AdminIssueListItemVm> Items { get; set; } = new();

        // NEW paging
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}
