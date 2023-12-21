using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RoaringView.IdentityModel;

namespace RoaringView.IdentityControllers
{
    public class LoginController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;

        public LoginController(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                return Ok();
            }
            return BadRequest("Invalid login attempt.");
        }
    }
}
