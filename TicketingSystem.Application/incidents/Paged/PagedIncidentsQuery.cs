using Ardalis.Result;
using MediatR;
using TicketingSystem.Application.Utilities;
using TicketingSystem.Core.Dtos.Incident;
using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.Application.incidents.Paged;

public record PagedIncidentsQuery(
    SupportStatus? SupportStatus,
    UserStatus? UserStatus,
    Module? Module,
    Priority? Priority,
    Guid? AssignedToId,
    DateTime? FromDate,
    DateTime? ToDate,
    int PageNumber = 1,
    int PageSize = 5
) : IRequest<Result<PagedList<IncidentSummaryDto>>>;