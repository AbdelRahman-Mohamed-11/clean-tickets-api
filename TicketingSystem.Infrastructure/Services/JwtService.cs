using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TicketingSystem.Application.Dtos.Identity;
using TicketingSystem.Core.Entities.Identity;
using TicketingSystem.Infrastructure.Interfaces;

namespace TicketingSystem.Infrastructure.Services
{
    public class JwtService(UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger<JwtService> logger,
        IConfiguration config) : IJwtService
    {
        public async Task<AuthResponseDto> GenerateJWTokenAsync(string userId)
        {
            var user = await userManager.FindByIdAsync(userId);

            var claims = new List<Claim>
            {
                new Claim("ID", userId),
                new Claim(ClaimTypes.Name, user!.UserName!),
            };

            var userClaims = await userManager.GetClaimsAsync(user);

            claims.AddRange(userClaims);

            var userRoles = await userManager.GetRolesAsync(user);

            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));

                var role = await roleManager.FindByNameAsync(userRole);


                var roleClaims = await roleManager.GetClaimsAsync(role); 

                foreach (Claim roleClaim in roleClaims)
                {
                    claims.Add(roleClaim);
                }
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = Encoding.UTF8.GetBytes(config["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(config["JWT:TokenExpireInMinutes"])),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha256Signature),
                Issuer = config["JWT:Issuer"],
                Audience = config["JWT:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            logger.LogInformation($"Generated JWT for user {user.UserName}:");
            logger.LogInformation($"Token: {tokenHandler.WriteToken(token)}");
            logger.LogInformation($"Expires at: {tokenDescriptor.Expires}");

            return new AuthResponseDto()
            {
                Token = tokenHandler.WriteToken(token),
                UserName = user.UserName,
                ExpireIn = tokenDescriptor.Expires
            };
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        public async Task<AuthResponseDto> RefreshAsync(UserRefreshToken userRefreshToken)
        {
            if (userRefreshToken is null)
            {
                throw new Exception("Invalid refresh token details");

            }

            string accessToken = userRefreshToken!.AccessToken!;
            string refreshToken = userRefreshToken.RefreshToken!;

            ClaimsPrincipal? principal = null;

            try
            {
                principal = GetPrincipalFromExpiredToken(accessToken);
            }
            catch (Exception)
            {
                throw new Exception("Invalid access token");

            }

            var username = principal!.Identity!.Name;


            ApplicationUser? user = null;

            if (username != null) user = await userManager.FindByNameAsync(username);

            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryDate <= DateTime.Now)
            {
                throw new Exception("Invalid client request");
            }

            var newAccessToken = await GenerateJWTokenAsync(user.Id.ToString());
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await userManager.UpdateAsync(user);


            logger.LogInformation($"User {username} successfully refreshed access token.");


            return new AuthResponseDto()
            {
                Token = newAccessToken.Token,
                RefreshToken = newRefreshToken,
                UserName = username,
                ExpireIn = newAccessToken.ExpireIn
            };
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var key = Encoding.UTF8.GetBytes(config["JWT:Key"]);

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["JWT:Issuer"],
                ValidAudience = config["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)

            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters,
                out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }
    }

}
