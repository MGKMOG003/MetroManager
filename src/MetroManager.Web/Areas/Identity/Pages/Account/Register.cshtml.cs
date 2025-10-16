using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MetroManager.Infrastructure.Identity;

namespace MetroManager.Web.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public RegisterModel(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty] public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required, EmailAddress] public string Email { get; set; } = "";
            [Required, DataType(DataType.Password)] public string Password { get; set; } = "";
            [DataType(DataType.Password), Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
            [Display(Name = "Full name")] public string? FullName { get; set; }
            [Display(Name = "Display name")] public string? DisplayName { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid) return Page();

            var user = new AppUser
            {
                UserName = Input.Email,
                Email = Input.Email,
                EmailConfirmed = true,
                FullName = Input.FullName ?? Input.Email,
                DisplayName = Input.DisplayName ?? Input.FullName ?? Input.Email
            };

            var create = await _userManager.CreateAsync(user, Input.Password);
            if (!create.Succeeded)
            {
                foreach (var e in create.Errors) ModelState.AddModelError(string.Empty, e.Description);
                return Page();
            }

            // Every self-registering user is a Client
            await _userManager.AddToRoleAsync(user, "Client");
            await _signInManager.SignInAsync(user, isPersistent: false);

            return LocalRedirect("/Dashboard");
        }
    }
}
