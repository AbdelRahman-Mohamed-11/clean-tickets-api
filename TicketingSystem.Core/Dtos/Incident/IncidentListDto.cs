using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.Core.Dtos.Incident
{
    public record IncidentSummaryDto(
       Guid Id,
       CallType CallType,
       Module Module,
       Priority Priority,
       SupportStatus SupportStatus,
       UserStatus UserStatus,
       string Subject,
       DateTime CreatedDate,
       Guid LoggedById,
       Guid? AssignedToId
      );
}
