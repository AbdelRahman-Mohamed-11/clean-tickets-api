using Ardalis.Result;
using MediatR;
using TicketingSystem.Core.Dtos.Incident;
using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.Application.incidents.List;

public record ListIncidentsQuery(
      SupportStatus? SupportStatus,
      UserStatus? UserStatus,
      Module? Module,
      Priority? Priority,
      Guid? AssignedToId,
      DateTime? FromDate,
      DateTime? ToDate
) : IRequest<Result<List<IncidentSummaryDto>>>;
