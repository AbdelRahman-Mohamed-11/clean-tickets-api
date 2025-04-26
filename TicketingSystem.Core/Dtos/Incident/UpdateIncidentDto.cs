using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.Core.Dtos.Incident;

public record UpdateIncidentDto(string? Suggestion , UserStatus? UserStatus , SupportStatus? SupportStatus ,
    Guid? AssignedToId , DateTime? DeliveryDate);


