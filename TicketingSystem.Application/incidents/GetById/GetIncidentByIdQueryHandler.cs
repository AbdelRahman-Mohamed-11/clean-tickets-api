using Ardalis.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TicketingSystem.Core.Attachments;
using TicketingSystem.Core.Comments;
using TicketingSystem.Core.Dtos.Incident;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Application.incidents.GetById;

public class GetIncidentByIdQueryHandler(
        ITicketDbContext db,
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
            .Include(i => i.Attachments)
            .FirstOrDefaultAsync(i => i.Id == request.Id, ct);

        if (incident is null)
        {
            logger.LogWarning("Incident not found: {IncidentId}", request.Id);
            return Result.NotFound("incidentNotFound");
        }

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
            AssignedToId: incident.AssignedToId,
            CreatedDate: incident.CreatedDate,
            DeliveryDate: incident.DeliveryDate,
            Comments: incident.Comments
                .Select(c => new GetCommentDto(c.Id, c.Text, c.CreatorId, c.CreatedAt))
                .ToList(),
            Attachments: incident.Attachments
                .Select(a => new GetAttachmentDto(a.Id, a.FileName, a.FilePath, a.UploadedAt))
                .ToList()
        );

        logger.LogInformation("Incident {IncidentId} fetched successfully", request.Id);
        return Result.Success(dto);
    }
}
