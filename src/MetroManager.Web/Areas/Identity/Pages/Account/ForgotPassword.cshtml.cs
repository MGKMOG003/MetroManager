using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using MetroManager.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MetroManager.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;

        public ForgotPasswordModel(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? InfoMessage { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; } = string.Empty;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user is not null && await _userManager.IsEmailConfirmedAsync(user))
            {
                // Generate token (in a real app you’d email this to the user)
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                // For local dev we don't send emails — show a neutral message:
            }

            InfoMessage = "If an account exists for that email, a reset link has been sent.";
            ModelState.Clear();
            return Page();
        }
    }
}
