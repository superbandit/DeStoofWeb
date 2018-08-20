using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Models.View.Users;


namespace DeStoofApi.Controllers
{
    [Authorize]
    [Route("api/account")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost, AllowAnonymous, Route("register")]
        public async Task<IActionResult> Register([FromBody]Register model)
        {
            try
            {
                var user = new IdentityUser {UserName = model.Email, Email = model.Email};
                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded) return BadRequest("Something went wrong while registering");

                await _signInManager.SignInAsync(user, false);
                return Ok();
            }
            catch(Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpPost, AllowAnonymous, Route("login")]
        public async Task<IActionResult> Login([FromBody]Login model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
            if (!result.Succeeded)
                return BadRequest("Could not log in with information");

            return Ok();            
        }

        [HttpPost, Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok();
        }

        [HttpPost, Route("test")]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}
