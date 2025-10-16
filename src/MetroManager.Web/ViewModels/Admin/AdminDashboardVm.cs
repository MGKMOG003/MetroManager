using System.Collections.Generic;
using MetroManager.Web.ViewModels.Dtos;

namespace MetroManager.Web.ViewModels.Admin
{
    public class AdminDashboardVm
    {
        public List<IssueDto> Issues { get; set; } = new();
        public List<UserDto> Users { get; set; } = new();
    }
}
