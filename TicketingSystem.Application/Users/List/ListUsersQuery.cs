using Ardalis.Result;
using MediatR;
using TicketingSystem.Core.Dtos.Identity;

namespace TicketingSystem.Application.Users.List;

public record ListUsersQuery() : IRequest<Result<List<GetUserDto>>>;


