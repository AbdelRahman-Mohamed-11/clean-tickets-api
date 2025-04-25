using System.Security.Claims;
using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketingSystem.Core.Attachments;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Application.IncidentAttachments.Add;

public class AddIncidentAttachmentsCommandHandler(
        ITicketDbContext db,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment env,
        ILogger<AddIncidentAttachmentsCommandHandler> logger
    ) : IRequestHandler<AddIncidentAttachmentsCommand, Result<List<GetAttachmentDto>>>
{
    public async Task<Result<List<GetAttachmentDto>>> Handle(
        AddIncidentAttachmentsCommand request,
        CancellationToken ct)
    {
        logger.LogInformation("Uploading {Count} attachments for incident {IncidentId}",
            request.Files.Count, request.IncidentId);

        var incident = await GetIncidentAsync(request.IncidentId, ct);
        if (incident is null)
            return Result.NotFound(
                "IncidentNotFound", $"Incident {request.IncidentId} does not exist.");

        var userId = GetUserIdFromToken();
        if (userId == null)
            return Result.Unauthorized(
                "Unauthorized", "Invalid or missing user token.");

        if (!IsAuthorized(userId.Value, incident))
            return Result.Unauthorized(
                "Forbidden", "You do not have permission to attach files to this incident.");

        var folder = EnsureUploadFolder(request.IncidentId);

        var dtos = await SaveAllFilesAsync(request.Files, request.IncidentId, userId.Value, folder, ct);

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Successfully uploaded {Count} attachments for incident {IncidentId}",
            dtos.Count, request.IncidentId);
        return Result.Success(dtos);
    }

    
    private async Task<Incident?> GetIncidentAsync(Guid incidentId, CancellationToken ct)
    {
        var incident = await db.Incidents
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == incidentId, ct);
        if (incident == null)
            logger.LogWarning("Incident not found: {IncidentId}", incidentId);
        return incident;
    }

    private Guid? GetUserIdFromToken()
    {
        var id = httpContextAccessor.HttpContext?
                     .User.FindFirst("ID")?
                     .Value;
        if (!Guid.TryParse(id, out var uid))
        {
            logger.LogWarning("Missing or invalid user ID in token");
            return null;
        }
        return uid;
    }

    private bool IsAuthorized(Guid userId, Incident incident)
    {
        var id = httpContextAccessor.HttpContext?
                                 .User.FindFirst("ID")?
                                 .Value;

        var isOwner = incident.LoggedById == Guid.Parse(id!);
        
        if (!isOwner)
            logger.LogWarning("User {UserId} not allowed to upload to incident {IncidentId}",
                userId, incident.Id);
        return isOwner;
    }

    private string EnsureUploadFolder(Guid incidentId)
    {
        var uploadsRoot = Path.Combine(env.WebRootPath, "uploads", "incidents", incidentId.ToString());
        Directory.CreateDirectory(uploadsRoot);
        return uploadsRoot;
    }

    private async Task<List<GetAttachmentDto>> SaveAllFilesAsync(
        List<IFormFile> files,
        Guid incidentId,
        Guid uploaderId,
        string folder,
        CancellationToken ct)
    {
        var results = new List<GetAttachmentDto>();
        foreach (var file in files)
        {
            var dto = await SaveSingleFileAsync(file, incidentId, uploaderId, folder, ct);
            results.Add(dto);
        }
        return results;
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
        logger.LogInformation("Saved file {FileName} for incident {IncidentId}", uniqueName, incidentId);

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
