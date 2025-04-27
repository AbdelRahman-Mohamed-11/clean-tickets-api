using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace TicketingSystem.Application.IncidentAttachments.Update;

public record UpdateIncidentAttachmentsCommand(
Guid IncidentId,
List<IFormFile> Files
) : IRequest<Result>;
