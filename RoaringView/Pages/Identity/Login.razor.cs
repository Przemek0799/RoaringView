using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace RoaringView.Pages.Identity
{
    public partial class Login
    {
        [Inject]
        private SignInManager<IdentityUser> SignInManager { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private UserManager<IdentityUser> UserManager { get; set; }

        [Inject]
        private IConfiguration Configuration { get; set; }

        [Inject]
        private ILogger<Login> _logger { get; set; }
        [Inject]
        private IJSRuntime JSRuntime { get; set; }

        public string Email { get; set; }
        private string password;
        private string error;

        private async Task LoginClicked()
        {
            error = null;
            var user = await UserManager.FindByEmailAsync(Email);
            if (user == null)
            {
                error = "User not found";
                return;
            }

            if (await SignInManager.CanSignInAsync(user))
            {
                var result = await SignInManager.CheckPasswordSignInAsync(user, password, true);
                if (result.Succeeded)
                {
                    Guid key = Guid.NewGuid();
                    BlazorCookieLoginMiddleware.Logins[key] = new LoginInfo { Email = Email, Password = password };

                    NavigationManager.NavigateTo($"/login?key={key}", true);
                }
                else
                {
                    error = "Login failed. Check your password.";
                }
            }
            else
            {
                error = "Your account is blocked";
            }
        }

       

    }
}
