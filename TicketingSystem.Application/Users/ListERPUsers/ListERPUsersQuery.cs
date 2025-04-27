using Ardalis.Result;
using MediatR;
using TicketingSystem.Core.Dtos.Identity;

namespace TicketingSystem.Application.Users.ListERPUsers;

public record ListERPUsersQuery() : IRequest<Result<List<GetUserDto>>>;


