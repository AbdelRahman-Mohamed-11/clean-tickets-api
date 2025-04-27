using Ardalis.Result;
using MediatR;
using TicketingSystem.Core.Dtos.Incident;

namespace TicketingSystem.Application.incidents.List;

public record ListIncidentsQuery : IRequest<Result<List<IncidentListDto>>>;
