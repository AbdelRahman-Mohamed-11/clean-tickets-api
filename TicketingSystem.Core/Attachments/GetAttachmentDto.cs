namespace TicketingSystem.Core.Attachments;

public record GetAttachmentDto(
       Guid Id,
       string FileName,
       string FilePath,
       DateTime UploadedAt
 );