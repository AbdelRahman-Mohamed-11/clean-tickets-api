using Ardalis.Result;
using MediatR;

namespace TicketingSystem.Application.Users.Register;

public record RegisterUserCommand(string UserName, string Email, string Password)
   : IRequest<Result<Guid>>;
