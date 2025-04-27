using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketingSystem.Core.Dtos.Identity;
using TicketingSystem.Core.Entities.Identity;

namespace TicketingSystem.Application.Users.List
{
    public class ListUsersQueryHandler(UserManager<ApplicationUser> userManager)
        : IRequestHandler<ListUsersQuery, Result<List<GetUserDto>>>
    {
        public async Task<Result<List<GetUserDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await userManager.Users
                .AsNoTracking()
                .Select(user => new GetUserDto(
                    user.Id,
                    user.UserName ?? string.Empty,
                    user.Email ?? string.Empty
                ))
                .ToListAsync(cancellationToken);

            return Result.Success(users);
        }
    }
}
