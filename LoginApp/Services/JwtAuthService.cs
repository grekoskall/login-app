using LoginApp.Models;
using LoginApp.Models.Users;
using LoginApp.Repositories.Common;
using LoginApp.Services.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LoginApp.Services
{
    public class JwtAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IEntityRepository _entityRepository;

        public JwtAuthService(IConfiguration configuration, IEntityRepository entityRepository)
        {
            _configuration = configuration;
            _entityRepository = entityRepository;
        }

        /// <summary>
        /// Method to generate JTW Token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public string GenerateJwtToken(UserEntity user, bool isTempToken = false)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.email),
                new Claim(ClaimTypes.Name, user.firstName),
                new Claim(ClaimTypes.Surname, user.lastName),
                new Claim(ClaimTypes.OtherPhone, user.telephone),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)            
            };
            if (!isTempToken)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Passed2fa"));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(isTempToken ? 60 : 60 * 24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Method to validate User's JWT token.
        /// </summary>
        /// <param name="token"></param>
        /// <returns>The User entity based on the JWT Identity</returns>
        public async Task<UserEntity?> ValidateJwtToken(string token)
        {
            try
            {
                var (principal, validatedToken) = await GetClaimPrincipal(token);
                if (validatedToken is JwtSecurityToken jwtToken && jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    var userId = principal?.Claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userId)) return null;

                    var user = await _entityRepository.FetchModelAsync(new UserEntity() { email = userId});
                    return user;
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(ClaimsPrincipal, SecurityToken?)> GetClaimPrincipal(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]);

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero // Adjust if needed
                };

                var principal = handler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                return (principal, validatedToken);
            }
            catch (Exception ex)
            {
                throw new UnauthorizedAccessException("Invalid Token");
            }
        }

        public async Task<string> GeneratePasswordResetTokenAsync(UserEntity user)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)); 
            var expires = DateTime.UtcNow.AddHours(1);

            var resetToken = new ResetTokenEntity
            {
                email = user.email,
                token = token,
                expiresAt = expires.ToString()
            };

            await _entityRepository.InsertOrUpdateModelAsync(resetToken, entity => entity.email == user.email);

            return token;
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var resetToken = await _entityRepository.GetModelByConditionAsync<ResetTokenEntity>(x => x.token == token);

            if (resetToken == null || DateTime.Parse(resetToken.expiresAt) < DateTime.UtcNow)
            {
                throw new Exception("Expired token"); 
            }

            var user = await _entityRepository.FetchModelAsync(new UserEntity() { email = resetToken.email });
            if (user == null) return false;

            var (passwordHash, salt) = HashPassword(newPassword);

            var userPwd = new UserPwdEntity
            {
                email = user.email,
                hash = passwordHash,
                salt = salt
            };

            await _entityRepository.UpdateModelAsync<UserPwdEntity>(userPwd);
            await _entityRepository.DeleteEntityAsync(resetToken);

            return true; 
        }

        public (string Hash, string Salt) HashPassword(string password)
        {
            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);
            return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
        }

    }
}
