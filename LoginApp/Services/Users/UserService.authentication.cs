using LoginApp.Models.Users;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LoginRequest = LoginApp.Models.Users.LoginRequest;

namespace LoginApp.Services
{
    public partial class UserService
    {
        public async Task<UserEntity> AuthenticateAsync(LoginRequest loginRequest)
        {
            ValidateUserLogin(loginRequest);

            _logger.LogInformation("User login attempt: {Email}", loginRequest.loginEmail);

            var user = await _entityRepository.FetchModelAsync(new UserEntity() { email = loginRequest.loginEmail });
            if (user is null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var userPwd = await _entityRepository.FetchModelAsync(new UserPwdEntity() { email = loginRequest.loginEmail });
            if (userPwd is null)
            {
                throw new UnauthorizedAccessException("Invalid email or password.");
            }

            var passwordMatch = VerifyPassword(loginRequest.loginPassword, userPwd.hash, userPwd.salt);
            if (!passwordMatch)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            return user;
        }

        public async Task SaveTempToken(UserEntity user, string tempToken)
        {
            if (user is null || string.IsNullOrEmpty(tempToken))
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
            var tempTokenEntity = new UserTempTokenEntity()
            {
                email = user.email,
                tempToken = tempToken,
                expirationDatetime = DateTime.UtcNow.AddMinutes(5).ToString(),
                codeFa = string.Empty,
                expirationFa = string.Empty
            };
            await _entityRepository.InsertOrUpdateModelAsync(tempTokenEntity, x => x.email == user.email);
        }

        public async Task DeleteTempToken(UserEntity user)
        {
            if (user is null)
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
            var tempTokenEntity = new UserTempTokenEntity()
            {
                email = user.email
            };
            await _entityRepository.DeleteEntityAsync(tempTokenEntity);
        }

        public async Task SaveFinalToken(UserEntity user, string finalToken)
        {
            if (user is null || string.IsNullOrEmpty(finalToken))
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
            var sessionEntity = new UserSessionEntity()
            {
                email = user.email,
                sessionToken = finalToken,
                expirationDatetime = DateTime.UtcNow.AddHours(24).ToString()
            };
            await _entityRepository.InsertOrUpdateModelAsync(sessionEntity, x => x.email == user.email);
        }

        public async Task<UserEntity> Verify2FA(string tempToken, TwoFactorRequest model)
        {
            if (string.IsNullOrEmpty(tempToken) || string.IsNullOrEmpty(model.TwoFactorCode))
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }

            var (principal, validatedToken) = await _jwtAuthService.GetClaimPrincipal(tempToken);
            if (validatedToken is JwtSecurityToken jwtToken && jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                var userId = principal?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("Unauthorized");
                var user = await _entityRepository.FetchModelAsync(new UserEntity() { email = userId });
                var validate2FA = await Validate2FACode(user.email, model.TwoFactorCode);
                if (!validate2FA) throw new UnauthorizedAccessException("Invalid Code");
                return user;
            } else
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
        }

        public async Task Save2FACode(UserEntity user, string twoFactorCode)
        {
            if (user is null || string.IsNullOrEmpty(twoFactorCode))
            {
                throw new UnauthorizedAccessException("Unauthorized");
            }
            var tempTokenEntity = await _entityRepository.FetchModelAsync(new UserTempTokenEntity() { email = user.email });
            if (tempTokenEntity is null) throw new UnauthorizedAccessException("Unauthorized");
            tempTokenEntity.codeFa = twoFactorCode;
            tempTokenEntity.expirationFa = DateTime.UtcNow.AddMinutes(60).ToString();
            await _entityRepository.UpdateModelAsync(tempTokenEntity);
        }

        public async Task<bool> Validate2FACode(string email, string submittedCode)
        {
            var user = await _entityRepository.FetchModelAsync(new UserTempTokenEntity() { email = email });
            if (user == null || string.IsNullOrEmpty(user.codeFa))
            {
                return false;
            }

            if (DateTime.Parse(user.expirationFa) < DateTime.UtcNow)
            {
                return false;
            }

            return user.codeFa == submittedCode;
        }

        public string Generate2FACode()
        {
            var random = new Random();
            var code = random.Next(100000, 999999).ToString(); 
            return code;
        }
    }
}