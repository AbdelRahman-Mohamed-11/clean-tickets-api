using Ardalis.Result;
using MediatR;
using TicketingSystem.Core.Comments;

namespace TicketingSystem.Application.IncidentComments.List;

public record ListCommentsQuery(
    Guid IncidentId
) : IRequest<Result<List<GetCommentDto>>>;
