using LoginApp.Models.Users;
using LoginApp.Services;
using LoginApp.Services.Users;
using Microsoft.AspNetCore.Mvc;

namespace LoginApp.Api.Users
{
    [ApiController]
    [Route("users")]
    public class UsersApiController : ControllerBase
    {

        private readonly IUserService _userService;
        private readonly JwtAuthService _jwtAuthService;
        private readonly MailService _mailService;


        public UsersApiController(IUserService userService, JwtAuthService jwtAuthService, MailService mailService)
        {
            _userService = userService;
            _jwtAuthService = jwtAuthService;
            _mailService = mailService;
        }


        [HttpGet]
        [Route("user-details")]
        public async Task<IActionResult> GetUserDetails()
        {
            var userCookie = Request.Cookies["Cookie"];
            if (string.IsNullOrEmpty(userCookie))
            {
                return Unauthorized(new { message = "Unauthorized." });
            }

            var user = await _jwtAuthService.ValidateJwtToken(userCookie);

            if (user == null)
            {
                return Unauthorized(new { message = "Unauthorized." });
            }

            return Ok(user);
        }

        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateNewUser([FromBody] UserEntity newUserInfo)
        {
            var userCookie = Request.Cookies["Cookie"];
            if (string.IsNullOrEmpty(userCookie)) return Unauthorized(new { message = "Unauthorized." });
            var user = await _jwtAuthService.ValidateJwtToken(userCookie);
            if (user is null) return Unauthorized(new { message = "Unauthorized." });

            var success = _userService.CreateNewUser(newUserInfo);

            return Ok(success);
        }

        [HttpPost]
        [Route("edit")]
        public async Task<IActionResult> UpdateUserDetails([FromBody] UpdateUserDTO newUserDetails)
        {
            var userCookie = Request.Cookies["Cookie"];
            if (string.IsNullOrEmpty(userCookie)) return Unauthorized(new { message = "Unauthorized." });
            var user = await _jwtAuthService.ValidateJwtToken(userCookie);
            if (user is null) return Unauthorized(new { message = "Unauthorized." });

            var success = _userService.UpdateUserDetails(user.email, newUserDetails);

            return Ok(success);
        }

        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetUserPassword([FromBody] PasswordResetRequest passwordReset)
        {
            var userCookie = Request.Cookies["Cookie"];
            if (string.IsNullOrEmpty(userCookie)) return Unauthorized(new { message = "Unauthorized." });
            var user = await _jwtAuthService.ValidateJwtToken(userCookie);
            if (user is null) return Unauthorized(new { message = "Unauthorized." });

            var success = _userService.ResetUserPassword(user.email, passwordReset);

            return Ok(success);
        }

        [HttpGet]
        [Route("login-instructions")]
        public async Task<IActionResult> SendLoginInstructions()
        {
            var userCookie = Request.Cookies["Cookie"];
            if (string.IsNullOrEmpty(userCookie)) return Unauthorized(new { message = "Unauthorized." });
            var user = await _jwtAuthService.ValidateJwtToken(userCookie);
            if (user is null) return Unauthorized(new { message = "Unauthorized." });

            var newTempPassword = await _userService.SendLoginInstructions(user.email);
            await _mailService.SendNewPasswordToEmail(user.email, newTempPassword);

            return Ok(true);
        }
    }
}
