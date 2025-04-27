namespace TicketingSystem.Core.Dtos.Incident;


public record IncidentListDto(
    Guid Id,
    string Subject,
    string CallRef
);
