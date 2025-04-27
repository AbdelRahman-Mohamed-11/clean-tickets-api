using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Identity;
using TicketingSystem.Core.Dtos.Identity;
using TicketingSystem.Core.Entities.Identity;

namespace TicketingSystem.Application.Users.GetUserById
{
    public class GetUserByIdQueryHandler(UserManager<ApplicationUser> userManager) : IRequestHandler<GetUserByIdQuery, Result<GetUserDto>>
    {
        public async Task<Result<GetUserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByIdAsync(request.Id.ToString());

            if (user is null)
            {
                return Result<GetUserDto>.NotFound($"User with ID {request.Id} not found.");
            }

            return Result<GetUserDto>.Success(new GetUserDto(user.Id , user.UserName ?? "" , user.Email ?? ""));
        }
    }
}
