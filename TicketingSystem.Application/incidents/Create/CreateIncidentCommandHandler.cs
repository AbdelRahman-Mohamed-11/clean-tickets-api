using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TicketingSystem.Core.Entities;
using TicketingSystem.Core.Interfaces;

namespace TicketingSystem.Application.incidents.Create;

public class CreateIncidentCommandHandler(ITicketDbContext ticketDbContext, IHttpContextAccessor httpContextAccessor)
        : IRequestHandler<CreateIncidentCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateIncidentCommand req, CancellationToken ct)
    {
        if (req.IsRecurring && req.RecurringCallId.HasValue)
        {
            var existingRecurringIncident = await ticketDbContext.Incidents
                .FindAsync(req.RecurringCallId.Value, ct);

            if (existingRecurringIncident is null)
            {
                return Result.NotFound($"Recurring incident with ID {req.RecurringCallId} not found.");
            }
        }

        var userId = httpContextAccessor
            .HttpContext?
            .User.FindFirst("ID")?
            .Value;

        if (userId is null)
        {
            return Result.Unauthorized("Invalid or missing user ID in token.");
        }

        var incident = new Incident(
            loggedById: Guid.Parse(userId),
            callType: req.CallType,
            module: req.Module,
            urlOrFormName: req.UrlOrFormName,
            isRecurring: req.IsRecurring,
            recurringCallId: req.RecurringCallId,
            priority: req.Priority,
            subject: req.Subject,
            description: req.Description,
            suggestion: req.Suggestion
        );

        await ticketDbContext.Incidents.AddAsync(incident , ct);

        await ticketDbContext.SaveChangesAsync(ct);

        return incident.Id;
    }
}