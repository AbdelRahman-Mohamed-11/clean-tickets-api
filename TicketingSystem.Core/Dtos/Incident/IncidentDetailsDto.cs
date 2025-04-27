using TicketingSystem.Core.Attachments;
using TicketingSystem.Core.Comments;
using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.Core.Dtos.Incident;

public record IncidentDetailsDto(
    Guid Id,
    CallType CallType,
    Module Module,
    string UrlOrFormName,
    bool IsRecurring,
    Guid? RecurringCallId,
    Priority Priority,
    string Subject,
    string Description,
    string? Suggestion,
    SupportStatus SupportStatus,
    UserStatus UserStatus,
    Guid LoggedById,
    string LoggedByUserName,
    Guid? AssignedToId,
    string? AssignedToUserName,
    DateTime CreatedDate,
    DateTime? DeliveryDate,
    IReadOnlyCollection<GetCommentDto> Comments,
    IReadOnlyCollection<GetAttachmentDto> Attachments
);
