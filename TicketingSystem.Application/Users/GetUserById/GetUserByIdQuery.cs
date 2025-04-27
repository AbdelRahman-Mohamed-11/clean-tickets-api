using Ardalis.Result;
using MediatR;
using TicketingSystem.Core.Dtos.Identity;

namespace TicketingSystem.Application.Users.GetUserById;

public record GetUserByIdQuery(
    Guid Id
) : IRequest<Result<GetUserDto>>;

