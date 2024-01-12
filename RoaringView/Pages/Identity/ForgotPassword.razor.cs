using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace RoaringView.Pages.Identity
{
    public partial class ForgotPassword
    {
        [Inject]
        private UserManager<IdentityUser> UserManager { get; set; }

        [Inject]
        private NavigationManager NavigationManager { get; set; }

        [Inject]
        private IEmailSender EmailSender { get; set; } // Assuming you have an email sender service

        [Inject]
        private ILogger<ForgotPassword> _logger { get; set; }

        private ForgotPasswordModel forgotPasswordModel = new ForgotPasswordModel();
        private bool emailSent = false;

        public class ForgotPasswordModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        private async Task HandleForgotPassword()
        {
            var user = await UserManager.FindByEmailAsync(forgotPasswordModel.Email);
            if (user == null)
            {
                _logger.LogWarning("User not found.");
                emailSent = true; // Optionally, decide if you want to reveal this information
                return;
            }

            var code = await UserManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(code));
            var callbackUrl = NavigationManager.ToAbsoluteUri($"/reset-password?code={code}").ToString();

            await EmailSender.SendEmailAsync(
                forgotPasswordModel.Email,
                "Reset Password",
                $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            emailSent = true;
        }
    }
}
