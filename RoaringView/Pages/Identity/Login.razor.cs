//using Microsoft.AspNetCore.Components;
//using Microsoft.AspNetCore.Identity;
//using System.ComponentModel.DataAnnotations;
//using System.Threading.Tasks;

//namespace RoaringView.Pages.Identity
//{
//    public partial class Login
//    {
//        [Inject]
//        private SignInManager<IdentityUser> SignInManager { get; set; }

//        [Inject]
//        private NavigationManager NavigationManager { get; set; }

//        [Inject]
//        private ILogger<Login> Logger { get; set; }

//        public InputModel Input { get; set; } = new InputModel();
//        public bool loginFailed { get; set; }

//        public class InputModel
//        {
//            [Required]
//            [EmailAddress]
//            public string Email { get; set; }

//            [Required]
//            [DataType(DataType.Password)]
//            public string Password { get; set; }
//        }

//        private async Task HandleLogin()
//        {
//            try
//            {
//                var result = await SignInManager.PasswordSignInAsync(Input.Email, Input.Password, isPersistent: false, lockoutOnFailure: false);

//                if (result.Succeeded)
//                {
//                    NavigationManager.NavigateTo("/add-company", true); // Force reload for navigation
//                }
//                else
//                {
//                    loginFailed = true;
//                }
//            }
//            catch (Exception ex)
//            {
//                Logger.LogError(ex, "Error during login process.");
//                loginFailed = true; // Optionally set loginFailed to true to display error message
//            }
//        }
//    }
//}
