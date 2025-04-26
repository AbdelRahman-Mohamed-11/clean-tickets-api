using System.IO;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketingSystem.Core.Attachments;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Application.IncidentAttachments.Update;

public class UpdateIncidentAttachmentsCommandHandler(
    ITicketDbContext db,
    IHttpContextAccessor httpContextAccessor,
    IWebHostEnvironment env,
    ILogger<UpdateIncidentAttachmentsCommandHandler> logger
) : IRequestHandler<UpdateIncidentAttachmentsCommand, Result>
{
    public async Task<Result> Handle(UpdateIncidentAttachmentsCommand request, CancellationToken ct)
    {
        logger.LogInformation("Updating attachments for Incident {IncidentId}", request.IncidentId);

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

        DeleteOldAttachmentsAsync(incident, ct);

        var folder = EnsureUploadFolder(request.IncidentId);
        
        await SaveAllFilesAsync(request.Files, request.IncidentId, userId.Value, folder, ct);

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Successfully updated attachments for Incident {IncidentId}", request.IncidentId);

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

    private bool IsAuthorized(Guid userId, Incident incident)
    {
        return incident.LoggedById == userId;
    }

    private void DeleteOldAttachmentsAsync(Incident incident, CancellationToken ct)
    {
        if (incident.Attachments is null || !incident.Attachments.Any())
            return;

        foreach (var attachment in incident.Attachments)
        {
            var fullPath = Path.Combine(env.WebRootPath, attachment.FilePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                logger.LogInformation("Deleted file {Path}", fullPath);
            }

            db.IncidentAttachments.Remove(attachment);
        }
    }

    private string EnsureUploadFolder(Guid incidentId)
    {
        var uploadsRoot = Path.Combine(env.WebRootPath, "uploads", "incidents", incidentId.ToString());
        Directory.CreateDirectory(uploadsRoot);
        return uploadsRoot;
    }

    private async Task SaveAllFilesAsync(
        List<IFormFile> files,
        Guid incidentId,
        Guid uploaderId,
        string folder,
        CancellationToken ct)
    {
        foreach (var file in files)
        {
            await SaveSingleFileAsync(file, incidentId, uploaderId, folder, ct);
        }
    }

    private async Task<GetAttachmentDto> SaveSingleFileAsync(
        IFormFile file,
        Guid incidentId,
        Guid uploaderId,
        string folder,
        CancellationToken ct)
    {
        var uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var fullPath = Path.Combine(folder, uniqueName);

        await using var fs = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(fs, ct);
        logger.LogInformation("Saved new file {FileName} for incident {IncidentId}", uniqueName, incidentId);

        var relativePath = Path.Combine("uploads", "incidents", incidentId.ToString(), uniqueName);

        var attachment = new IncidentAttachment(incidentId, file.FileName, relativePath, uploaderId);
        await db.IncidentAttachments.AddAsync(attachment, ct);

        return new GetAttachmentDto(
            attachment.Id,
            attachment.FileName,
            attachment.FilePath,
            attachment.UploadedAt
        );
    }
}
