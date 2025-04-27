using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Application.IncidentComments.Add
{
    public class AddIncidentCommentsCommandHandler(
        ITicketDbContext db,
        IHttpContextAccessor http,
        ILogger<AddIncidentCommentsCommandHandler> logger)
                : IRequestHandler<AddIncidentCommentsCommand, Result<List<Guid>>>
    {
        public async Task<Result<List<Guid>>> Handle(
            AddIncidentCommentsCommand req,
            CancellationToken ct)
        {
            logger.LogInformation(
                "Adding {Count} comments to Incident {Id}",
                req.Comments.Count,
                req.IncidentId);

            var incident = await db.Incidents
                .FirstOrDefaultAsync(i => i.Id == req.IncidentId, ct);
            if (incident is null)
            {
                logger.LogWarning(
                    "Incident not found: {Id}",
                    req.IncidentId);
                return Result.NotFound(
                    "IncidentNotFound",
                    $"Incident {req.IncidentId} does not exist.");
            }

            var userIdStr = http.HttpContext?.User.FindFirst("ID")?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                logger.LogWarning("Invalid or missing user ID in token");
                return Result.Unauthorized(
                    "Unauthorized",
                    "Invalid user token.");
            }

            var user = http.HttpContext!.User;
            var isAdmin = user.IsInRole("Admin");
            var isErp = user.IsInRole("ERP");
            var isOwner = incident.LoggedById == userId;
            if (!isAdmin && !isErp && !isOwner)
            {
                logger.LogWarning(
                    "User {UserId} not permitted to comment on Incident {Id}",
                    userId,
                    req.IncidentId);
                return Result.Unauthorized(
                    "Forbidden",
                    "You do not have permission to comment on this incident.");
            }

            var comments = req.Comments
                .Select(text => new IncidentComment(req.IncidentId, text, userId))
                .ToList();

            await db.IncidentComments.AddRangeAsync(comments, ct);
            
            await db.SaveChangesAsync(ct);

            var ids = comments.Select(c => c.Id).ToList();
            logger.LogInformation(
                "Created comments {CommentIds} on Incident {Id}",
                string.Join(", ", ids),
                req.IncidentId);

            return Result.Success(ids);
        }
    }
}
