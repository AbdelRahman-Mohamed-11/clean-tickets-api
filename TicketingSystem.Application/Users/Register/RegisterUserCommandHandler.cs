using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TicketingSystem.Core.Entities.Identity;
using TicketingSystem.Core.Entities.Identity.Enums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TicketingSystem.Application.Users.Register
{
    public class RegisterUserCommandHandler(UserManager<ApplicationUser> userManager, ILogger<RegisterUserCommandHandler> logger)
       : IRequestHandler<RegisterUserCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RegisterUserCommand req, CancellationToken ct)
        {
            logger.LogInformation("Attempting to register new user {UserName}", req.UserName);

            var user = new ApplicationUser
            {
                UserName = req.UserName,
                Email = req.Email
            };

            var result = await userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded)
            {
                logger.LogWarning(
                    "User registration failed for {UserName}: {Errors}",
                    req.UserName,
                    string.Join("; ", result.Errors)
                );

                return Result.Error(new ErrorList(result.Errors.Select(e => e.Description)));
            }

            await userManager.AddToRoleAsync(user, nameof(Role.User));

            logger.LogInformation("User {UserName} created successfully with ID {UserId}", user.UserName, user.Id);

            return Result.Created(user.Id);
        }
    }
}
