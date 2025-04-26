using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Application.IncidentComments
{
    public class AddIncidentCommentCommandHandler(
            ITicketDbContext db,
            IHttpContextAccessor http,
            ILogger<AddIncidentCommentCommandHandler> logger
        ) : IRequestHandler<AddIncidentCommentCommand, Result<Guid>>
    {
        public async Task<Result<Guid>> Handle(
            AddIncidentCommentCommand req,
            CancellationToken ct)
        {
            logger.LogInformation("Adding comment to Incident {Id}: {Text}", req.IncidentId, req.Text);

            var incident = await db.Incidents
                .FirstOrDefaultAsync(i => i.Id == req.IncidentId, ct);
            if (incident is null)
            {
                logger.LogWarning("Incident not found: {Id}", req.IncidentId);
                return Result.NotFound("IncidentNotFound", $"Incident {req.IncidentId} does not exist.");
            }

            var userIdStr = http.HttpContext?.User.FindFirst("ID")?.Value;
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                logger.LogWarning("Invalid or missing user ID in token");
                return Result.Unauthorized("Unauthorized", "Invalid user token.");
            }

            var user = http.HttpContext!.User;
            var isAdmin = user.IsInRole("Admin");
            var isErp = user.IsInRole("ERP");
            var isOwner = incident.LoggedById == userId;
            if (!isAdmin && !isErp && !isOwner)
            {
                logger.LogWarning("User {UserId} not permitted to comment on Incident {Id}", userId, req.IncidentId);
                return Result.Unauthorized("Forbidden", "You do not have permission to comment on this incident.");
            }

            var comment = new IncidentComment(req.IncidentId, req.Text, userId);
            await db.IncidentComments.AddAsync(comment, ct);
            
            await db.SaveChangesAsync(ct);

           

            logger.LogInformation("Comment {CommentId} created on Incident {Id}", comment.Id, req.IncidentId);
            return Result.NoContent();
        }
    }
}
