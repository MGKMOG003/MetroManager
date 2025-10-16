using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MetroManager.Infrastructure.Identity
{
    public class AppUser : IdentityUser
    {
        [PersonalData, MaxLength(100)]
        public string? DisplayName { get; set; }

        // Back-compat for any code that still selects FullName
        [PersonalData, MaxLength(150)]
        public string? FullName { get; set; }
    }
}
