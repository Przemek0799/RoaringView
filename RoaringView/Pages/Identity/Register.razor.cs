using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace RoaringView.Pages.Identity
{
    public partial class Register
    {
        [Inject]
        private UserManager<IdentityUser> UserManager { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private ILogger<Register> Logger { get; set; }
        public InputModel Input { get; set; } = new InputModel();

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }
        }

        private async Task HandleRegister()
        {
            var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
            var result = await UserManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                Logger.LogInformation("Registration successful. Redirecting to /register.");
                NavigationManager.NavigateTo("/add-company");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Logger.LogError("Registration failed: {Error}", error.Description);
                    // Handle errors
                }
            }
        }
    }
}
