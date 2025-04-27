using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Application.IncidentAttachments.Remove;

public class RemoveIncidentAttachmentsCommandHandler(
    ITicketDbContext db,
    IHttpContextAccessor httpContextAccessor,
    IWebHostEnvironment env,
    ILogger<RemoveIncidentAttachmentsCommandHandler> logger
) : IRequestHandler<RemoveIncidentAttachmentsCommand, Result>
{
    public async Task<Result> Handle(RemoveIncidentAttachmentsCommand request, CancellationToken ct)
    {
        logger.LogInformation("Removing attachments for Incident {IncidentId}", request.IncidentId);

        var incident = await db.Incidents
            .Include(i => i.Attachments)
            .FirstOrDefaultAsync(i => i.Id == request.IncidentId, ct);

        if (incident is null)
        {
            logger.LogWarning("Incident not found: {IncidentId}", request.IncidentId);
            return Result.NotFound("IncidentNotFound", $"Incident {request.IncidentId} does not exist.");
        }

        var userId = GetUserIdFromToken();
        if (userId == null)
            return Result.Unauthorized("Unauthorized", "Invalid or missing user token.");

        if (!IsAuthorized(userId.Value, incident))
            return Result.Unauthorized("Forbidden", "You do not have permission to update attachments for this incident.");

        var attachmentsToRemove = incident.Attachments
            .Where(a => request.AttachmentIds.Contains(a.Id))
            .ToList();

        if (!attachmentsToRemove.Any())
        {
            logger.LogWarning("No matching attachments found to remove");
            return Result.Success(); 
        }

        foreach (var attachment in attachmentsToRemove)
        {
            var fullPath = Path.Combine(env.WebRootPath, attachment.FilePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                logger.LogInformation("Deleted file {Path}", fullPath);
            }

            db.IncidentAttachments.Remove(attachment);
        }

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Successfully removed {Count} attachments for Incident {IncidentId}",
            attachmentsToRemove.Count, request.IncidentId);

        return Result.Success();
    }

    private Guid? GetUserIdFromToken()
    {
        var id = httpContextAccessor.HttpContext?.User.FindFirst("ID")?.Value;
        if (!Guid.TryParse(id, out var uid))
        {
            logger.LogWarning("Missing or invalid user ID in token");
            return null;
        }
        return uid;
    }

    private bool IsAuthorized(Guid userId, Core.Entities.Incident incident)
    {
        return incident.LoggedById == userId;
    }
}