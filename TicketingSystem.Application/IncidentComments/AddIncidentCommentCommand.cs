using Ardalis.Result;
using MediatR;


namespace TicketingSystem.Application.IncidentComments;

public record AddIncidentCommentCommand(
    Guid IncidentId,
    string Text
) : IRequest<Result<Guid>>;
