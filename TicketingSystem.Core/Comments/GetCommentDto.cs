namespace TicketingSystem.Core.Comments;

public record GetCommentDto(
    Guid Id,
    string Text,
    string CreatedBy,
    DateTime CreatedAt
);
