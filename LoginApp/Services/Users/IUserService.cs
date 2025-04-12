using LoginApp.Models.Users;
using Microsoft.AspNetCore.Identity.Data;
using LoginRequest = LoginApp.Models.Users.LoginRequest;

namespace LoginApp.Services.Users
{
    public interface IUserService
    {
        public Task<UserEntity> AuthenticateUser(LoginRequest loginRequest);
        public Task SaveTempToken(UserEntity user, string tempToken);
        public Task DeleteTempToken(UserEntity user);
        public Task SaveFinalToken(UserEntity user, string finalToken);
        public Task<UserEntity> Verify2FA(string token, TwoFactorRequest model);
        public Task Save2FACode(UserEntity user, string twoFactorCode);
        public string Generate2FACode();
        public Task<bool> CreateNewUser(UserEntity newUserInfo);
        public Task<bool> UpdateUserDetails(string userEmail, UpdateUserDTO newUserDetails);
        public Task<bool> ResetUserPassword(string userEmail, PasswordResetRequest passwordReset);
        public Task<string> SendLoginInstructions(string userEmail);
        public Task<UserEntity> FetchUser(string email);

    }
}
