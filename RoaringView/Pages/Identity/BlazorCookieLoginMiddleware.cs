using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace RoaringView.Pages.Identity
{
    //https://github.com/dotnet/aspnetcore/issues/13601
    public class LoginInfo
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }

    public class BlazorCookieLoginMiddleware
    {
        public static IDictionary<Guid, LoginInfo> Logins { get; private set; }
            = new ConcurrentDictionary<Guid, LoginInfo>();


        private readonly RequestDelegate _next;
        [Inject]
        private ILogger<BlazorCookieLoginMiddleware> _logger { get; set; }
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _RoaringjwtKey;


        public BlazorCookieLoginMiddleware(RequestDelegate next, ILogger<BlazorCookieLoginMiddleware> logger, IHttpContextAccessor httpContextAccessor, string RoaringjwtKey)
        {
            _next = next;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _RoaringjwtKey = RoaringjwtKey;
        }
        public async Task Invoke(HttpContext context, SignInManager<IdentityUser> signInMgr)
        {
            _logger.LogInformation("BlazorCookieLoginMiddleware invoked.");

            if (context.Request.Path == "/login" && context.Request.Query.ContainsKey("key"))
            {
                var key = Guid.Parse(context.Request.Query["key"]);
                var info = Logins[key];

                var result = await signInMgr.PasswordSignInAsync(info.Email, info.Password, false, lockoutOnFailure: true);
                _logger.LogInformation($"SignIn result: {result.Succeeded}, Requires 2FA: {result.RequiresTwoFactor}");

                info.Password = null;
                if (result.Succeeded)
                {
                    var user = await signInMgr.UserManager.FindByEmailAsync(info.Email);
                    var jwtToken = GenerateJwtToken(user); // Assume you have a method to generate the token

                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Expires = DateTime.UtcNow.AddHours(1)
                    };
                    context.Response.Cookies.Append("jwtToken", jwtToken, cookieOptions);

                    Logins.Remove(key);
                    context.Response.Redirect("/add-company");
                    return;
                }
                else if (result.RequiresTwoFactor)
                {
                    //TODO: redirect to 2FA razor component
                    context.Response.Redirect("/loginwith2fa/" + key);
                    return;
                }

                else
                {
                    //TODO: Proper error handling
                    context.Response.Redirect("/loginfailed");
                    return;
                }
            }
            else if (context.Request.Path.StartsWithSegments("/logout"))
            {
                await signInMgr.SignOutAsync();
                context.Response.Redirect("/login");
                return;
            } 

            {
                await _next.Invoke(context);
            }
        }
        public string GenerateJwtToken(IdentityUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_RoaringjwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "RoaringView",
                audience: "RoaringAPI",
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            var generatedToken = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation($"Generated JWT Token: {generatedToken}");
            return generatedToken;
        }

    }
}