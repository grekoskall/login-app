using LoginApp.Models.Users;
using LoginApp.Repositories.Common;
using LoginApp.Services.Users;

namespace LoginApp.Services
{
    public partial class UserService : IUserService
    {
        private readonly IEntityRepository _entityRepository;
        private readonly ILogger<UserService> _logger;
        private readonly JwtAuthService _jwtAuthService;

        public UserService(
            IEntityRepository entityRepository,
            ILogger<UserService> logger,
            JwtAuthService jwtAuthService
            )
        {
            _entityRepository = entityRepository;
            _logger = logger;
            _jwtAuthService = jwtAuthService;
        }

        public async Task<UserEntity> AuthenticateUser(LoginRequest loginRequest)
        {
            var user = await AuthenticateAsync(loginRequest);
            if (user is null)
            {
                throw new UnauthorizedAccessException("Invalid credentials.");
            }
            return user;
        }

        public async Task<bool> CreateNewUser(UserEntity newUserInfo)
        {
            ValidateUserCreation(newUserInfo);

            var existingUser = await _entityRepository.GetModelByConditionAsync<UserEntity>(u => u.email == newUserInfo.email);
            if (existingUser != null) throw new Exception("User with this email already exists.");

            var (passwordHash, salt) = _jwtAuthService.HashPassword(newUserInfo.password);

            await _entityRepository.InsertModelAsync<UserEntity>(newUserInfo);

            var userPwd = new UserPwdEntity
            {
                email = newUserInfo.email,
                hash = passwordHash,
                salt = salt
            };

            await _entityRepository.InsertModelAsync<UserPwdEntity>(userPwd);

            return true;
        }

        public async Task<bool> UpdateUserDetails(string userEmail, UpdateUserDTO newUserDetails)
        {
            ValidateUpdateDetails(newUserDetails);


            var persistedUser = await _entityRepository.FetchModelAsync(new UserEntity() { email = userEmail });
            if (persistedUser == null) throw new Exception("No User found");

            persistedUser.firstName = newUserDetails.firstName;
            persistedUser.lastName = newUserDetails.lastName;
            persistedUser.telephone = newUserDetails.telephone;
            persistedUser.photoPath = newUserDetails.photoPath;

            await _entityRepository.UpdateModelAsync<UserEntity>(persistedUser);

            return true;
        }

        public async Task<UserEntity> FetchUser(string email)
        {
            var user = await _entityRepository.FetchModelAsync(new UserEntity() { email = email });
            return user;
        }

        public async Task<bool> ResetUserPassword(string userEmail, PasswordResetRequest passwordReset)
        {
            ValidatePasswordReset(passwordReset);

            var userPwd = await _entityRepository.FetchModelAsync(new UserPwdEntity() { email = userEmail });
            if (userPwd == null) throw new Exception("Data mismatch");

            if (!VerifyPassword(passwordReset.oldPassword, userPwd.hash, userPwd.salt)) throw new Exception("Incorrect Password");

            var (newPasswordHash, newSalt) = _jwtAuthService.HashPassword(passwordReset.password);

            userPwd.hash = newPasswordHash;
            userPwd.salt = newSalt;

            await _entityRepository.UpdateModelAsync<UserPwdEntity>(userPwd);

            return true;
        }

        public async Task<string> SendLoginInstructions(string userEmail)
        {
            var userPwd = await _entityRepository.FetchModelAsync(new UserPwdEntity() { email = userEmail });
            if (userPwd == null) throw new Exception("Data mismatch");

            var newTempPassword = Generate2FACode();
            var (newPasswordHash, newSalt) = _jwtAuthService.HashPassword(newTempPassword);
            userPwd.hash = newPasswordHash;
            userPwd.salt = newSalt;
            await _entityRepository.UpdateModelAsync<UserPwdEntity>(userPwd);
            return newTempPassword;
        }

    }
}
