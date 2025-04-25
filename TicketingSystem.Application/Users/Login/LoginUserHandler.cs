using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TicketingSystem.Core.Dtos.Identity;
using TicketingSystem.Core.Entities.Identity;
using TicketingSystem.Infrastructure.Interfaces;

namespace TicketingSystem.Application.Users.Login
{
    public class LoginUserCommandHandler(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration config,
        IJwtService jwtService,
        ILogger<LoginUserCommandHandler> logger) : IRequestHandler<LoginUserCommand, Result<AuthResponseDto>>
    {
        public async Task<Result<AuthResponseDto>> Handle(
            LoginUserCommand request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Login attempt for {Email}", request.Email);
            var user = await FindUserByEmailAsync(request.Email);
            if (user is null)
            {
                logger.LogWarning("Login failed: user not found ({Email})", request.Email);
                return Result.Unauthorized("UserNotAuthorized");
            }

            var credsOk = await CheckPasswordAsync(user, request.Password);
            if (!credsOk)
            {
                logger.LogWarning("Login failed: invalid password for {Email}", request.Email);
                return Result.Unauthorized("UserNotAuthorized");
            }

            var refreshToken = GenerateRefreshToken();
            await PersistRefreshTokenAsync(user, refreshToken);

            var authResponse = await BuildAuthResponseAsync(user, refreshToken);

            logger.LogInformation("User {Email} logged in successfully", request.Email);

            return Result.Success(authResponse);
        }

        private Task<ApplicationUser?> FindUserByEmailAsync(string email)
            => userManager.FindByEmailAsync(email);

        private async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            var result = await signInManager
                .CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
            return result.Succeeded;
        }

        private string GenerateRefreshToken()
            => jwtService.GenerateRefreshToken();

        private async Task PersistRefreshTokenAsync(ApplicationUser user, string refreshToken)
        {
            var expiresIn = double.Parse(config["JWT:RefreshTokenExpireInMinutes"]!);
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryDate = DateTime.UtcNow.AddMinutes(expiresIn);
            await userManager.UpdateAsync(user);
        }

        private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user, string refreshToken)
        {
            var auth = await jwtService.GenerateJWTokenAsync(user.Id.ToString());
            auth.RefreshToken = refreshToken;
            auth.Roles = await userManager.GetRolesAsync(user);
            return auth;
        }
    }
}
