using LoginApp.Models.Users;

namespace LoginApp.Services
{
    public partial class UserService
    {

        private void ValidateUserLogin(LoginRequest loginRequest)
        {
            if (loginRequest == null ||
                string.IsNullOrEmpty(loginRequest.loginEmail) ||
                string.IsNullOrEmpty(loginRequest.loginPassword)
                )
            {
                _logger.LogInformation("Invalid login attempt");
                throw new UnauthorizedAccessException("Unathorized");
            }
        }

        private void ValidateUserCreation(UserEntity newUserInfo)
        {
            if (newUserInfo is null || string.IsNullOrEmpty(newUserInfo.email) || string.IsNullOrEmpty(newUserInfo.password))
            {
                throw new Exception("Invalid request");
            }
        }

        private void ValidateUpdateDetails(UpdateUserDTO newUserDetails)
        {
            if (newUserDetails is null )
            {
                throw new Exception("Invalid request");
            }
        }

        private void ValidatePasswordReset(PasswordResetRequest passwordReset)
        {
            if (passwordReset is null || string.IsNullOrEmpty(passwordReset.oldPassword) || string.IsNullOrEmpty(passwordReset.password)) {
                throw new Exception("Invalid request");
            }
        }
    }
}
