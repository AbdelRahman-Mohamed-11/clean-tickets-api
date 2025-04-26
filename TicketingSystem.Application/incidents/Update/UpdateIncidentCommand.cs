using Ardalis.Result;
using MediatR;
using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.Application.incidents.Update;

public record UpdateIncidentCommand(
    Guid Id,
    string? Suggestion,
    UserStatus? UserStatus,
    SupportStatus? SupportStatus,
    Guid? AssignedToId,
    DateTime? DeliveryDate
) : IRequest<Result>;
