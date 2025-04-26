using Ardalis.Result;
using MediatR;


namespace TicketingSystem.Application.IncidentComments.Add;

public record AddIncidentCommentCommand(
    Guid IncidentId,
    string Text
) : IRequest<Result<Guid>>;
