using Ardalis.Result;
using MediatR;
using TicketingSystem.Core.Dtos.Incident;

namespace TicketingSystem.Application.incidents.GetById;

public record GetIncidentByIdQuery(Guid Id)
        : IRequest<Result<IncidentDetailsDto>>;