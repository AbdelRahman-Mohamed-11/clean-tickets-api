using System;
using System.Linq;
using System.Threading.Tasks;
using Ardalis.Result;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TicketingSystem.Application.IncidentAttachments.Add;
using TicketingSystem.Application.incidents.Create;
using TicketingSystem.Application.incidents.GetById;
using TicketingSystem.Application.incidents.List;
using TicketingSystem.Core.Attachments;
using TicketingSystem.Core.Dtos.Incident;
using TicketingSystem.Core.Dtos.IncidentAttachments;
using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.Api.Controllers
{
    [ApiController]
    [Route("api/incidents")]
    [Authorize]
    public class IncidentsController(
        IMediator mediator,
        IValidator<CreateIncidentCommand> createValidator,
        IValidator<AddIncidentAttachmentsCommand> attachValidator,
        ILogger<CreateIncidentCommandHandler> createLogger,
        ILogger<GetIncidentByIdQueryHandler> getLogger,
        ILogger<AddIncidentAttachmentsCommandHandler> attachLogger
    ) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateIncident([FromBody] CreateIncidentDto dto)
        {
            createLogger.LogInformation("Received CreateIncident request: {@Dto}", dto);

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

            var validation = await createValidator.ValidateAsync(cmd);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => e.ErrorMessage);
                createLogger.LogWarning("Validation failed for CreateIncident: {Errors}", string.Join("; ", errors));
                return BadRequest(Result.Invalid(new List<ValidationError>(errors.Select(e => new ValidationError(e)))));
            }

            var result = await mediator.Send(cmd);

            if (result.Status == ResultStatus.Unauthorized)
            {
                createLogger.LogWarning("Unauthorized CreateIncident attempt");
                return Unauthorized();
            }

            if (result.Status == ResultStatus.NotFound)
            {
                createLogger.LogWarning("CreateIncident parent not found: {Errors}", result.Errors);
                return NotFound(result.Errors);
            }

            if (result.IsSuccess)
            {
                createLogger.LogInformation("Incident created with ID {Id}", result.Value);
                return CreatedAtAction(
                    nameof(GetIncidentById),
                    new { id = result.Value },
                    new { id = result.Value }
                );
            }

            createLogger.LogWarning("Error creating incident: {Errors}", result.Errors);
            return BadRequest(result.Errors);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(IncidentDetailsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetIncidentById(Guid id)
        {
            getLogger.LogInformation("Received GetIncidentById request for ID {Id}", id);

            var result = await mediator.Send(new GetIncidentByIdQuery(id));

            if (result.Status == ResultStatus.NotFound)
            {
                getLogger.LogWarning("Incident not found: {Id}", id);
                return NotFound(result.Errors);
            }

            getLogger.LogInformation("Returning Incident {Id}", id);
            return Ok(result.Value);
        }

        [HttpPost("{id:guid}/attachments")]
        [RequestSizeLimit(50_000_000)]
        [ProducesResponseType(typeof(List<GetAttachmentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddIncidentAttachments(
            Guid id,
            [FromForm] AddAttachmentsDto addAttachmentsDto)
        {
            attachLogger.LogInformation("Received AddAttachments request for Incident {Id}", id);

            var cmd = new AddIncidentAttachmentsCommand(id, addAttachmentsDto.Files);

            var validation = await attachValidator.ValidateAsync(cmd);
            if (!validation.IsValid)
            {
                var errors = validation.Errors.Select(e => e.ErrorMessage);
                attachLogger.LogWarning("AddAttachments validation failed: {Errors}", string.Join("; ", errors));
                return BadRequest(Result.Invalid(new List<ValidationError>(errors.Select(e => new ValidationError(e)))));
            }

            var result = await mediator.Send(cmd);

            if (result.Status == ResultStatus.Unauthorized)
            {
                attachLogger.LogWarning("Unauthorized AddAttachments attempt for Incident {Id}", id);
                return Unauthorized();
            }

            if (result.Status == ResultStatus.NotFound)
            {
                attachLogger.LogWarning("Incident not found for AddAttachments: {Id}", id);
                return NotFound(result.Errors);
            }

            if (result.IsSuccess)
            {
                attachLogger.LogInformation(
                    "Uploaded {Count} attachments for Incident {Id}", result.Value.Count, id);

                return CreatedAtAction(
                    nameof(GetIncidentById),
                    new { id },
                    result.Value
                );
            }

            attachLogger.LogWarning("AddAttachments error for Incident {Id}: {Errors}", id, result.Errors);
            return BadRequest(result.Errors);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<IncidentSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetIncidents(
              [FromQuery] SupportStatus? supportStatus,
              [FromQuery] UserStatus? userStatus,
              [FromQuery] Module? module,
              [FromQuery] Priority? priority,
              [FromQuery] Guid? assignedToId,
              [FromQuery] DateTime? fromDate,
              [FromQuery] DateTime? toDate)
        {
            var query = new ListIncidentsQuery(
              supportStatus,
              userStatus,
              module,
              priority,
              assignedToId,
              fromDate,
              toDate);

            var result = await mediator.Send(query);

            return result.IsSuccess
              ? Ok(result.Value)
              : BadRequest(result.Errors);
        }
    }
}

