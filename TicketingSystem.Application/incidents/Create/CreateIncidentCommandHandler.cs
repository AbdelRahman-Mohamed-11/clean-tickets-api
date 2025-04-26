using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Application.incidents.Create;

public class CreateIncidentCommandHandler(ITicketDbContext ticketDbContext, IHttpContextAccessor httpContextAccessor, 
    ILogger<CreateIncidentCommandHandler> logger)
        : IRequestHandler<CreateIncidentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateIncidentCommand request, CancellationToken ct)
    {
        logger.LogInformation("Starting incident creation. IsRecurring={IsRecurring}, Subject={Subject}",
                request.IsRecurring, request.Subject);

        if (request.IsRecurring && request.RecurringCallId.HasValue)
        {
            var existingRecurringIncident = await ticketDbContext.Incidents
                .FindAsync(request.RecurringCallId.Value, ct);

            if (existingRecurringIncident is null)
            {
                logger.LogWarning("Recurring incident not found: {RecurringCallId}", request.RecurringCallId);

                return Result.NotFound($"Recurring incident with ID {request.RecurringCallId} not found.");
            }
        }

        var userId = httpContextAccessor
            .HttpContext?
            .User.FindFirst("ID")?
            .Value;

        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var _))
        {
            logger.LogWarning("Unauthorized: missing or invalid user ID in token");
            return Result.Unauthorized("InvalidOrMissingUserId");
        }

        var incident = new Incident(
            loggedById: Guid.Parse(userId),
            callType: request.CallType,
            module: request.Module,
            urlOrFormName: request.UrlOrFormName,
            isRecurring: request.IsRecurring,
            recurringCallId: request.RecurringCallId,
            priority: request.Priority,
            subject: request.Subject,
            description: request.Description,
            suggestion: request.Suggestion
        );

        await ticketDbContext.Incidents.AddAsync(incident, ct);

        await ticketDbContext.SaveChangesAsync(ct);


        logger.LogInformation("Incident created successfully with ID {IncidentId} by user {UserId}",
            incident.Id, userId);

        return Result.Created(incident.Id);
    }
}