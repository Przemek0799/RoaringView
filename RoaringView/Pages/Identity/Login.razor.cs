using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

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
        private ILogger<Login> Logger { get; set; }



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
                if (result == Microsoft.AspNetCore.Identity.SignInResult.Success)
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
