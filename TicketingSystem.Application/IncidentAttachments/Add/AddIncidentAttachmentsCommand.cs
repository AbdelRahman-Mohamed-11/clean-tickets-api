using Ardalis.Result;
using MediatR;
using Microsoft.AspNetCore.Http;
using TicketingSystem.Core.Attachments;

namespace TicketingSystem.Application.IncidentAttachments.Add;

public record AddIncidentAttachmentsCommand(
Guid IncidentId,
List<IFormFile> Files
) : IRequest<Result<List<GetAttachmentDto>>>;
