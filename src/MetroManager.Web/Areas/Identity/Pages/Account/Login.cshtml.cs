using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MetroManager.Infrastructure.Identity;

namespace MetroManager.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [BindProperty] public InputModel Input { get; set; } = new();

        public class InputModel
        {
            public string Email { get; set; } = "";
            public string Password { get; set; } = "";
            public bool RememberMe { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid) return Page();

            var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user != null)
            {
                if (await _userManager.IsInRoleAsync(user, "Admin"))
                    return LocalRedirect("/Admin");
                if (await _userManager.IsInRoleAsync(user, "Client"))
                    return LocalRedirect("/Dashboard");
            }

            // fallback
            return LocalRedirect(returnUrl ?? "/");
        }
    }
}
