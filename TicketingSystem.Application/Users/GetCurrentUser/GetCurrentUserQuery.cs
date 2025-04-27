using Ardalis.Result;
using MediatR;
using TicketingSystem.Core.Dtos.Identity;

namespace TicketingSystem.Application.Users.GetCurrentUser;

public record GetCurrentUserQuery(
) : IRequest<Result<GetUserDto>>;
