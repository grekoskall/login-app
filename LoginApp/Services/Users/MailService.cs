using LoginApp.Contracts;
using LoginApp.Models.Users;
using LoginApp.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http;
using System.Net.Mail;

namespace LoginApp.Services.Users
{
    public class MailService
    {
        private readonly IUserService _userService;
        private readonly GmailOptions _gmailOptions;

        public MailService(IUserService userService, IOptions<GmailOptions> gmailOptions)
        {
            _userService = userService;
            _gmailOptions = gmailOptions.Value;
        }

        public async Task GenerateAndSend2FACode(UserEntity user)
        {
            if (user is null || string.IsNullOrEmpty(user.email))
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
            var twoFactorCode = _userService.Generate2FACode();
            await _userService.Save2FACode(user, twoFactorCode);
            var emailRequest = new SendEmailRequest
            (
                user.email,
                "LoginApp: 2FA Code",
                "Your 2FA code is: " + twoFactorCode
            );
            await Send2FACode(user, emailRequest);
        }

        public async Task SendNewPasswordToEmail(string toEmail, string newPassword)
        {
            if (string.IsNullOrEmpty(toEmail)) throw new Exception("Invalid Request");
            var emailRequest = new SendEmailRequest(
                toEmail,
                "LoginApp: Login Instructions - New Password",
                "Your password has been reset. Use this temporary password to login: " + newPassword
            );
            await SendLoginInstructions(toEmail, emailRequest);
        }

        public async Task SendResetEmailAsync(string email, string token)
        {
            var resetLink = $"https://localhost:5033/v1/login/reset-password.html?token={Uri.EscapeDataString(token)}";

            if (string.IsNullOrEmpty(email)) throw new Exception("Invalid Request");
            var emailRequest = new SendEmailRequest(
                email,
                "LoginApp: Password Reset",
                $"Click <a href='{resetLink}'>here</a> to reset your password."
                );

            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(_gmailOptions.Email),
                Subject = emailRequest.Subject,
                Body = emailRequest.Body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            using var smtpClient = new SmtpClient();
            smtpClient.Host = _gmailOptions.Host;
            smtpClient.Port = _gmailOptions.Port;
            smtpClient.Credentials = new NetworkCredential(_gmailOptions.Email, _gmailOptions.Password);
            smtpClient.EnableSsl = true;
            await smtpClient.SendMailAsync(mailMessage);
        }

        private async Task Send2FACode(UserEntity user, SendEmailRequest sendEmailRequest)
        {
            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(_gmailOptions.Email),
                Subject = sendEmailRequest.Subject,
                Body = sendEmailRequest.Body
            };

            mailMessage.To.Add(user.email);

            using var smtpClient = new SmtpClient();
            smtpClient.Host = _gmailOptions.Host;
            smtpClient.Port = _gmailOptions.Port;
            smtpClient.Credentials = new NetworkCredential(_gmailOptions.Email, _gmailOptions.Password);
            smtpClient.EnableSsl = true;
            await smtpClient.SendMailAsync(mailMessage);
        }

        private async Task SendLoginInstructions(string toEmail, SendEmailRequest sendEmailRequest)
        {
            MailMessage mailMessage = new MailMessage()
            {
                From = new MailAddress(_gmailOptions.Email),
                Subject = sendEmailRequest.Subject,
                Body = sendEmailRequest.Body
            };

            mailMessage.To.Add(toEmail);

            using var smtpClient = new SmtpClient();
            smtpClient.Host = _gmailOptions.Host;
            smtpClient.Port = _gmailOptions.Port;
            smtpClient.Credentials = new NetworkCredential(_gmailOptions.Email, _gmailOptions.Password);
            smtpClient.EnableSsl = true;
            await smtpClient.SendMailAsync(mailMessage);
        }

       

    }
}
