using Ardalis.Result;
using MediatR;


namespace TicketingSystem.Application.IncidentComments.Add;

public record AddIncidentCommentsCommand(
       Guid IncidentId,
       List<string> Comments
   ) : IRequest<Result<List<Guid>>>;
