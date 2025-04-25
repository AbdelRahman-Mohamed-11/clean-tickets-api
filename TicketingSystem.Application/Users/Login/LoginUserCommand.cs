using Ardalis.Result;
using MediatR;
using TicketingSystem.Core.Dtos.Identity;

namespace TicketingSystem.Application.Users.Login;

public record LoginUserCommand(string Email, string Password)
  : IRequest<Result<AuthResponseDto>>;
