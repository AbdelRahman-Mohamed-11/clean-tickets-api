using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using TicketingSystem.Core.Dtos.Identity;
using TicketingSystem.Core.Entities.Identity;

namespace TicketingSystem.Application.Users.GetCurrentUser
{
    public class GetCurrentUserQueryHandler(UserManager<ApplicationUser> userManager , RoleManager<ApplicationRole> roleManager, IHttpContextAccessor httpContextAccessor) : IRequestHandler<GetCurrentUserQuery, Result<GetUserDto>>
    {
        public async Task<Result<GetUserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var id = httpContextAccessor.HttpContext!.User.FindFirstValue("ID");

            var user = await userManager.FindByIdAsync(id!.ToString());

            if (user is null)
            {
                return Result<GetUserDto>.NotFound($"User with ID {id} not found.");
            }

            var roles = await userManager.GetRolesAsync(user);

            return Result<GetUserDto>.Success(new GetUserDto(user.Id, user.UserName ?? "", user.Email ?? "") { Roles = roles});
        }
    }
}
