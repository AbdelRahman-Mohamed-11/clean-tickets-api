using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketingSystem.Core.Comments;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Application.IncidentComments.List
{
    public class ListCommentsQueryHandler(
        ITicketDbContext dbContext,
        ILogger<ListCommentsQueryHandler> logger) : IRequestHandler<ListCommentsQuery,Result<List<GetCommentDto>>>
    {
        public async Task<Result<List<GetCommentDto>>> Handle(
            ListCommentsQuery request,
            CancellationToken cancellationToken)
        {
            logger.LogInformation("Fetching comments for Incident {IncidentId}", request.IncidentId);

            var comments = await dbContext.IncidentComments
                .AsNoTracking()
                .Where(c => c.IncidentId == request.IncidentId)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new GetCommentDto(
                    c.Id,
                    c.Text,
                    c.CreatorId,
                    c.CreatedAt))
                .ToListAsync(cancellationToken);

            return Result.Success(comments);
        }
    }
}
