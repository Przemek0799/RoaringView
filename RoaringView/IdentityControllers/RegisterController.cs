using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RoaringView.IdentityModel;

namespace RoaringView.IdentityControllers
{
    public class RegisterController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public RegisterController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new IdentityUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Optionally sign-in the user after registration
                return Ok();
            }
            return BadRequest(result.Errors);
        }
    }
}
