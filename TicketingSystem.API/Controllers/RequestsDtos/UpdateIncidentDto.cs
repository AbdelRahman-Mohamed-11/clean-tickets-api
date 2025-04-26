using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.API.Controllers.RequestsDtos;

public record UpdateIncidentDto(string? Suggestion, UserStatus? UserStatus, SupportStatus? SupportStatus,
    Guid? AssignedToId, DateTime? DeliveryDate);


