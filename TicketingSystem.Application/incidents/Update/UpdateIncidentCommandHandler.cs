using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TicketingSystem.Application.incidents.Update;
using TicketingSystem.Core.Entities.Identity.Enums;
using TicketingSystem.Core.Entities.Identity;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace TicketingSystem.Application.Incidents.Update;

public class UpdateIncidentCommandHandler(
    ITicketDbContext db,
    IHttpContextAccessor http,
    UserManager<ApplicationUser> userManager,
    ILogger<UpdateIncidentCommandHandler> logger
) : IRequestHandler<UpdateIncidentCommand, Result>
{
    public async Task<Result> Handle(UpdateIncidentCommand req, CancellationToken ct)
    {
        logger.LogInformation("Updating Incident {Id} with {@Update}", req.Id, req);

        var incident = await db.Incidents.FirstOrDefaultAsync(i => i.Id == req.Id, ct);
        if (incident is null)
        {
            logger.LogWarning("Incident not found: {Id}", req.Id);
            return Result.NotFound("IncidentNotFound", $"Incident {req.Id} not found.");
        }

        var userResult = GetUserIdAndRoles();
        if (!userResult.IsSuccess)
            return userResult.Result!;

        var (userId, isAdmin, isErp, isOwner) = userResult.Data;

        var validationResult = await ValidateAndUpdateIncident(req, incident, userId, isAdmin, isErp, isOwner, ct);
        if (!validationResult.IsSuccess)
            return validationResult.Result!;

        incident.SetStatusUpdatedDate(DateTime.UtcNow);

        await db.SaveChangesAsync(ct);

        logger.LogInformation("Incident {Id} updated successfully", req.Id);
        
        return Result.NoContent();
    }

    private (bool IsSuccess, Result? Result, (Guid userId, bool isAdmin, bool isErp, bool isOwner) Data) GetUserIdAndRoles()
    {
        var user = http.HttpContext?.User;
        var userIdClaim = user?.FindFirst("ID")?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            logger.LogWarning("Missing or invalid user ID in token");
            return (false, Result.Unauthorized("Unauthorized", "Invalid user token."), default);
        }

        return (true, null, (
            userId,
            user!.IsInRole("Admin"),
            user.IsInRole("ERP"),
            false 
        ));
    }

    private async Task<(bool IsSuccess, Result? Result)> ValidateAndUpdateIncident(
        UpdateIncidentCommand req,
        Incident incident,
        Guid userId,
        bool isAdmin,
        bool isErp,
        bool isOwner,
        CancellationToken ct)
    {
        isOwner = incident.LoggedById == userId;

        if (req.Suggestion is not null || req.UserStatus is not null)
        {
            if (!isOwner && !isErp && !isAdmin)
            {
                logger.LogWarning("Unauthorized update attempt for Suggestion or UserStatus.");
                return (false, Result.Unauthorized("Forbidden", "Cannot change suggestion or user status."));
            }

            incident.SetSuggestion(req.Suggestion);
            if (req.UserStatus.HasValue)
            {
                incident.SetUserStatus(req.UserStatus.Value);
                if (req.UserStatus == UserStatus.Closed)
                    incident.SetClosedDate(DateTime.UtcNow);
            }
        }

        if (req.SupportStatus.HasValue)
        {
            if (!isErp && !isAdmin)
            {
                logger.LogWarning("Unauthorized support status update.");
                return (false, Result.Unauthorized("Forbidden", "Cannot change support status."));
            }
            incident.SetSupportStatus(req.SupportStatus.Value);
        }

        if (req.AssignedToId.HasValue)
        {
            if (!isAdmin)
            {
                logger.LogWarning("Unauthorized incident assignment attempt.");
                return (false, Result.Unauthorized("Forbidden", "Cannot assign incident."));
            }

            var assignedUser = await userManager.FindByIdAsync(req.AssignedToId.ToString()!);
            if (assignedUser is null)
                return (false, Result.NotFound("UserNotFound", $"User {req.AssignedToId} not found."));

            incident.SetAssignedTo(req.AssignedToId.Value);
        }

        if (req.DeliveryDate.HasValue)
        {
            var createdDate = await db.Incidents
                .AsNoTracking()
                .Where(i => i.Id == req.Id)
                .Select(i => i.CreatedDate)
                .FirstOrDefaultAsync(ct);

            if (req.DeliveryDate.Value < createdDate)
            {
                logger.LogWarning("Invalid DeliveryDate for Incident {Id}", req.Id);
                return (false, Result.Invalid(new ValidationError
                {
                    ErrorMessage = "DeliveryDate cannot be earlier than the incident’s creation date."
                }));
            }

            if (!isOwner && !isErp && !isAdmin)
            {
                logger.LogWarning("Unauthorized delivery date set attempt.");
                return (false, Result.Unauthorized("Forbidden", "Cannot set delivery date."));
            }

            incident.SetDeliveryDate(req.DeliveryDate.Value);
        }

        return (true, null);
    }
}
