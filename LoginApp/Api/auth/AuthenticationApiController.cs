using LoginApp.Services;
using LoginApp.Services.Users;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using LoginRequest = LoginApp.Models.Users.LoginRequest;
using LoginApp.Models;
using ResetPasswordRequest = LoginApp.Models.ResetPasswordRequest;
using ForgotPasswordRequest = LoginApp.Models.ForgotPasswordRequest;

[ApiController]
[Route("v1/auth")]
public class AuthenticationApiController : ControllerBase
{
    private readonly JwtAuthService _jwtAuthService;
    private readonly MailService _mailService;
    private readonly IUserService _userService;

    public AuthenticationApiController(JwtAuthService jwtAuthService, IUserService userService, MailService mailService)
    {
        _jwtAuthService = jwtAuthService;
        _mailService = mailService;
        _userService = userService;
    }

    [HttpPost("authenticate")]
    public async Task<IActionResult> AuthenticateUser([FromBody] LoginRequest loginRequest)
    {
        var user = await _userService.AuthenticateUser(loginRequest);
        if (user is null)
        {
            return Unauthorized("Unauthorized");
        }

        var tempToken = _jwtAuthService.GenerateJwtToken(user, isTempToken: true);
        await _userService.SaveTempToken(user, tempToken);

        await _mailService.GenerateAndSend2FACode(user);

        Response.Cookies.Append("TempCookie", tempToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddMinutes(60)
        });
        return Ok(new { message = "2FA required" });
    }

    [HttpPost("verify-2fa")]
    public async Task<IActionResult> Verify2FA([FromBody] TwoFactorRequest model)
    {
        var tempToken = Request.Cookies["TempCookie"];
        var user = await _userService.Verify2FA(tempToken, model);
        if (user is null)
        {
            return Unauthorized("Unauthorized");
        }

        var finalToken = _jwtAuthService.GenerateJwtToken(user);
        await _userService.SaveFinalToken(user, finalToken);

        Response.Cookies.Delete("TempCookie");
        await _userService.DeleteTempToken(user);
        Response.Cookies.Append("Cookie", finalToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(24)
        });
        return Ok(new { message = "2FA successful" });
    }

    [HttpPost("logout-user")]
    public async Task<IActionResult> LogoutUser()
    {
        Response.Cookies.Append("TempCookie", "", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddMinutes(-1),
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict
        });
        Response.Cookies.Append("Cookie", "", new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddMinutes(-1),
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict
        });
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost]
    [Route("forgot-password")]
    public async Task<IActionResult> forgotPasswordReset([FromBody] ForgotPasswordRequest model)
    {
        var user = await _userService.FetchUser(model.email);
        if (user is null) throw new Exception("Invalid email");

        var token = await _jwtAuthService.GeneratePasswordResetTokenAsync(user);
        await _mailService.SendResetEmailAsync(user.email, token);

        return Ok(new { message = "Link sent to your email." });
    }

    [HttpPost]
    [Route("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest model)
    {
        if (string.IsNullOrWhiteSpace(model.Token) || string.IsNullOrWhiteSpace(model.NewPassword))
        {
            return BadRequest(new { message = "Token and new password are required." });
        }

        var success = await _jwtAuthService.ResetPasswordAsync(model.Token, model.NewPassword);

        if (!success)
        {
            return BadRequest(new { message = "Invalid or expired token." });
        }

        return Ok(new { message = "Password has been reset successfully." });
    }

}
