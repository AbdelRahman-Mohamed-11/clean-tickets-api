using Ardalis.Result;
using MediatR;

namespace TicketingSystem.Application.IncidentAttachments.Remove;

public record RemoveIncidentAttachmentsCommand(
 Guid IncidentId,
 List<Guid> AttachmentIds
) : IRequest<Result>;
