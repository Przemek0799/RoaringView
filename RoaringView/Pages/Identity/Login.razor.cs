using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;

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
        private ILogger<Login> _logger { get; set; }

        public string Email { get; set; }
        private string password;
        private string error;

        private async Task LoginClicked()
        {
            error = null;
            var user = await UserManager.FindByEmailAsync(Email);
            _logger.LogInformation("Login attempt for user {Email}", Email);

            if (user == null)
            {
                error = "User not found";
                _logger.LogError("Error during login: {ErrorMessage}", error);

                return;
            }

            if (await SignInManager.CanSignInAsync(user))
            {
                var result = await SignInManager.CheckPasswordSignInAsync(user, password, true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Login successful for {Email}", Email);

                    Guid key = Guid.NewGuid();
                    BlazorCookieLoginMiddleware.Logins[key] = new LoginInfo { Email = Email, Password = password };

                    NavigationManager.NavigateTo($"/login?key={key}", true);
                }
                else
                {
                    _logger.LogError("Login failed for {Email}", Email);

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
