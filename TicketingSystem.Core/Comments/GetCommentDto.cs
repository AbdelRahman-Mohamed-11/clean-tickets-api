namespace TicketingSystem.Core.Comments;

public record GetCommentDto(
    Guid Id,
    string Text,
    Guid CreatorId,
    DateTime CreatedAt
);
