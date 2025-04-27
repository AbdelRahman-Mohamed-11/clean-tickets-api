using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketingSystem.Core.Attachments;
using TicketingSystem.Core.Comments;
using TicketingSystem.Core.Dtos.Incident;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Entities.Identity;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Application.incidents.GetById;

public class GetIncidentByIdQueryHandler(
        ITicketDbContext db,
        IHttpContextAccessor httpContextAccessor,
        UserManager<ApplicationUser> userManager,
        ILogger<GetIncidentByIdQueryHandler> logger
    ) : IRequestHandler<GetIncidentByIdQuery, Result<IncidentDetailsDto>>
{
    public async Task<Result<IncidentDetailsDto>> Handle(
        GetIncidentByIdQuery request,
        CancellationToken ct)
    {
        logger.LogInformation("Fetching incident with ID {IncidentId}", request.Id);

        var incident = await db.Incidents
            .AsNoTracking()
            .Include(i => i.Comments)
                .ThenInclude(i => i.Creator)
            .Include(incident => incident.LoggedBy)
            .Include(incident => incident.AssignedTo)
            .Include(i => i.Attachments)
            .FirstOrDefaultAsync(i => i.Id == request.Id, ct);

        if (incident is null)
        {
            logger.LogWarning("Incident not found: {IncidentId}", request.Id);
            return Result.NotFound("incidentNotFound");
        }

        var userValidation = await ValidateUserAuthorization(incident, ct);
        if (!userValidation.IsSuccess)
        {
            return userValidation;
        }

        var httpRequest = httpContextAccessor.HttpContext?.Request;
        
        var baseUrl = $"{httpRequest!.Scheme}://{httpRequest.Host}";

        var dto = new IncidentDetailsDto(
            Id: incident.Id,
            CallType: incident.CallType,
            Module: incident.Module,
            UrlOrFormName: incident.UrlOrFormName,
            IsRecurring: incident.IsRecurring,
            RecurringCallId: incident.RecurringCallId,
            Priority: incident.Priority,
            Subject: incident.Subject,
            Description: incident.Description,
            Suggestion: incident.Suggestion,
            SupportStatus: incident.SupportStatus,
            UserStatus: incident.UserStatus,
            LoggedById: incident.LoggedById,
            LoggedByUserName: incident.LoggedBy.UserName!,
            AssignedToId: incident.AssignedToId,
            AssignedToUserName: incident.AssignedTo?.UserName,
            CreatedDate: incident.CreatedDate,
            DeliveryDate: incident.DeliveryDate,
            Comments: incident.Comments
                .Select(c => new GetCommentDto(c.Id, c.Text, c.Creator.UserName!, c.CreatedAt))
                .ToList(),
            Attachments: incident.Attachments
                .Select(a => new GetAttachmentDto(a.Id, a.FileName, $"{baseUrl}/{a.FilePath}", a.UploadedAt))
                .ToList()
        );

        logger.LogInformation("Incident {IncidentId} fetched successfully", request.Id);
        return Result.Success(dto);
    }

    private async Task<Result<IncidentDetailsDto>> ValidateUserAuthorization(Incident incident, CancellationToken ct)
    {
        var user = httpContextAccessor.HttpContext?.User;
        var userIdClaim = user?.FindFirst("ID")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            logger.LogWarning("Invalid or missing user ID in token");
            return Result.Unauthorized("Invalid or missing user ID");
        }

        if (incident.LoggedById == userId)
        {
            return Result.Success();
        }

        var appUser = await userManager.FindByIdAsync(userId.ToString());
        if (appUser == null)
        {
            logger.LogWarning("User {UserId} not found", userId);
            return Result.Unauthorized("User not found");
        }

        var roles = await userManager.GetRolesAsync(appUser);
        if (roles.Contains("Admin") || roles.Contains("ERP"))
        {
            return Result.Success();
        }

        logger.LogWarning("User {UserId} not authorized to access incident {IncidentId}", userId, incident.Id);
        return Result.Unauthorized("User not authorized to access this incident");
    }
}
