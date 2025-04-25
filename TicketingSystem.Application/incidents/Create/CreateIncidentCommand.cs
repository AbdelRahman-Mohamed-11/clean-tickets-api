using Ardalis.Result;
using MediatR;
using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.Application.incidents.Create;

public record CreateIncidentCommand(
        CallType CallType,
        Module Module,
        string UrlOrFormName,
        bool IsRecurring,
        Guid? RecurringCallId,
        Priority Priority,
        string Subject,
        string Description,
        string? Suggestion
    ) : IRequest<Result<Guid>>;
