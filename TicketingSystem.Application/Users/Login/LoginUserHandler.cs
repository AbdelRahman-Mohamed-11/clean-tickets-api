using System;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using TicketingSystem.Core.Dtos.Identity;
using TicketingSystem.Core.Entities.Identity;
using TicketingSystem.Infrastructure.Interfaces;

namespace TicketingSystem.Application.Users.Login
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<AuthResponseDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly IJwtService _jwtService;

        public LoginUserCommandHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config,
            IJwtService jwtService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _jwtService = jwtService;
        }

        public async Task<Result<AuthResponseDto>> Handle(
            LoginUserCommand request,
            CancellationToken cancellationToken)
        {
            var user = await FindUserByEmailAsync(request.Email);
            if (user is null) return Result.Unauthorized("UserNotAuthorized");

            var credsOk = await CheckPasswordAsync(user, request.Password);
            if (!credsOk) return Result.Unauthorized("UserNotAuthorized");

            var refreshToken = GenerateRefreshToken();
            await PersistRefreshTokenAsync(user, refreshToken);

            var authResponse = await BuildAuthResponseAsync(user, refreshToken);

            return Result.Success(authResponse);
        }

        private Task<ApplicationUser?> FindUserByEmailAsync(string email)
            => _userManager.FindByEmailAsync(email);

        private async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
        {
            var result = await _signInManager
                .CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
            return result.Succeeded;
        }

        private string GenerateRefreshToken()
            => _jwtService.GenerateRefreshToken();

        private async Task PersistRefreshTokenAsync(ApplicationUser user, string refreshToken)
        {
            var expiresIn = double.Parse(_config["JWT:RefreshTokenExpireInMinutes"]!);
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryDate = DateTime.UtcNow.AddMinutes(expiresIn);
            await _userManager.UpdateAsync(user);
        }

        private async Task<AuthResponseDto> BuildAuthResponseAsync(ApplicationUser user, string refreshToken)
        {
            var auth = await _jwtService.GenerateJWTokenAsync(user.Id.ToString());
            auth.RefreshToken = refreshToken;
            auth.Roles = await _userManager.GetRolesAsync(user);
            return auth;
        }
    }
}
