using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TicketingSystem.Core.Entities.Identity;
using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.Application.Users.Register
{
    public class RegisterUserCommandHandler(UserManager<ApplicationUser> userManager)
       : IRequestHandler<RegisterUserCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(RegisterUserCommand req, CancellationToken ct)
        {
            var user = new ApplicationUser
            {
                UserName = req.UserName,
                Email = req.Email
            };

            var result = await userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded)
            {
                return Result.Error(new ErrorList(result.Errors.Select(e => e.Description)));
            }

            await userManager.AddToRoleAsync(user, nameof(Role.User));

            return Result.Created(user.Id);
        }
    }
}
