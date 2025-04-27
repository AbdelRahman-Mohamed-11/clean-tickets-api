using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TicketingSystem.Core.Dtos.Identity;
using TicketingSystem.Core.Entities.Identity;
using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.Application.Users.ListERPUsers
{
    public class ListERPUsersQueryHandler(UserManager<ApplicationUser> userManager)
        : IRequestHandler<ListERPUsersQuery, Result<List<GetUserDto>>>
    {
        public async Task<Result<List<GetUserDto>>> Handle(ListERPUsersQuery request, CancellationToken cancellationToken)
        {
            var erpUsers = await userManager.GetUsersInRoleAsync(nameof(Role.ERP));

            var dtos = erpUsers
                  .Select(u => new GetUserDto(
                      u.Id,
                      u.UserName ?? string.Empty,
                      u.Email ?? string.Empty
                  ))
                  .ToList();

            return Result.Success(dtos);
        }
    }
}
