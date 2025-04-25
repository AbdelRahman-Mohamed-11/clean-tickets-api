using Ardalis.Result;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using TicketingSystem.Application.incidents.Create;
using TicketingSystem.Core.Dtos.Incident;

namespace TicketingSystem.Api.Endpoints
{
    public static class IncidentEndpoints
    {
        public static RouteGroupBuilder MapIncidentEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/incidents")
                           .RequireAuthorization()
                           .WithTags("Incidents");

            group.MapPost("/", async (
                    CreateIncidentDto dto,
                    IValidator<CreateIncidentCommand> validator,
                    IMediator mediator) =>
            {
                var cmd = new CreateIncidentCommand(
                    dto.CallType,
                    dto.Module,
                    dto.UrlOrFormName,
                    dto.IsRecurring,
                    dto.RecurringCallId,
                    dto.Priority,
                    dto.Subject,
                    dto.Description,
                    dto.Suggestion
                );

                var validation = await validator.ValidateAsync(cmd);
                if (!validation.IsValid)
                    return Results.ValidationProblem(validation.ToDictionary());

                var result = await mediator.Send(cmd);

                if (result.Status == ResultStatus.Unauthorized)
                    return Results.Unauthorized();

                if (result.Status == ResultStatus.NotFound)
                    return Results.NotFound(result.Errors);

                if (result.IsSuccess)
                    return Results.Created($"/api/incidents/{result.Value}", new { id = result.Value });

                return Results.BadRequest(result.Errors);
            })
                .WithName("CreateIncident")
                .Produces<Guid>(StatusCodes.Status201Created)
                .ProducesValidationProblem()
                .Produces(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound);

            return group;
        }
    }
}
