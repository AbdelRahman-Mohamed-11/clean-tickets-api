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
using TicketingSystem.API.Controllers.RequestsDtos;
using TicketingSystem.API.Extensions;
using TicketingSystem.Application.IncidentAttachments.Add;
using TicketingSystem.Application.IncidentAttachments.Remove;
using TicketingSystem.Application.IncidentAttachments.Update;
using TicketingSystem.Application.IncidentComments.Add;
using TicketingSystem.Application.IncidentComments.List;
using TicketingSystem.Application.incidents.Create;
using TicketingSystem.Application.incidents.GetById;
using TicketingSystem.Application.incidents.List;
using TicketingSystem.Application.incidents.Paged;
using TicketingSystem.Application.incidents.Update;
using TicketingSystem.Core.Attachments;
using TicketingSystem.Core.Constatns;
using TicketingSystem.Core.Dtos.Incident;
using TicketingSystem.Core.Dtos.IncidentAttachments;
using TicketingSystem.Core.Entities.Identity.Enums;

namespace TicketingSystem.Api.Controllers;

[ApiController]
[Route("api/incidents")]
[Authorize]
public class IncidentsController(
    IMediator mediator,
    IValidator<CreateIncidentCommand> createValidator,
    IValidator<AddIncidentAttachmentsCommand> attachValidator,
    IValidator<UpdateIncidentCommand> updateIncidentCommandValidator,
    IValidator<UpdateIncidentAttachmentsCommand> updateIncidentAttachmentsCommandValidator,
    IValidator<AddIncidentCommentsCommand> addIncidentCommentCommandValidator,
    IValidator<RemoveIncidentAttachmentsCommand> removeIncidentAttachmentsCommandValidator,
    ILogger<IncidentsController> logger
) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateIncident([FromBody] CreateIncidentDto dto)
    {
        using var scope = logger.BeginScope("CreateIncident: {CorrelationId}", Guid.NewGuid());

        logger.LogInformation("Processing CreateIncident request: {@Dto}", dto);

        var command = IncidentMapper.ToCommand(dto);

        var validationResult = await createValidator.ValidateAsync(command);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            logger.LogWarning("Validation failed: {Errors}", string.Join("; ", errors));
            return BadRequest(Result.Invalid(errors.Select(e => new ValidationError(e)).ToList()));
        }

        var result = await mediator.Send(command);

        return result.Status switch
        {
            ResultStatus.Unauthorized => Unauthorized(),
            ResultStatus.NotFound => NotFound(result.Errors),
            ResultStatus.Created => CreatedAtAction(
                nameof(GetIncidentById),
                new { id = result.Value },
                new { id = result.Value }),
            _ => BadRequest(result.Errors)
        };
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IncidentDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetIncidentById(Guid id)
    {
        using var scope = logger.BeginScope("GetIncidentById: {CorrelationId}", Guid.NewGuid());

        logger.LogInformation("Processing request for incident ID {IncidentId}", id);

        var result = await mediator.Send(new GetIncidentByIdQuery(id));

        if (result.Status == ResultStatus.NotFound)
        {
            logger.LogWarning("Incident not found: {IncidentId}", id);
            return NotFound(result.Errors);
        }

        logger.LogInformation("Successfully retrieved incident {IncidentId}", id);

        return Ok(result.Value);
    }

    [HttpPost("{id:guid}/attachments")]
    [RequestSizeLimit(FileConstants.MaxUploadSize)]
    [ProducesResponseType(typeof(List<GetAttachmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddIncidentAttachments(Guid id, [FromForm] AddAttachmentsDto addAttachmentsDto)
    {
        using var scope = logger.BeginScope("AddIncidentAttachments: {CorrelationId}", Guid.NewGuid());
        logger.LogInformation("Processing attachment upload for incident {IncidentId}", id);


        var command = new AddIncidentAttachmentsCommand(id, addAttachmentsDto.Files);
        var validationResult = await attachValidator.ValidateAsync(command);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            logger.LogWarning("Validation failed: {Errors}", string.Join("; ", errors));
            return BadRequest(Result.Invalid(errors.Select(e => new ValidationError(e)).ToList()));
        }

        var result = await mediator.Send(command);

        return result.Status switch
        {
            ResultStatus.Unauthorized => Unauthorized(),
            ResultStatus.NotFound => NotFound(result.Errors),
            ResultStatus.Ok => CreatedAtAction(
                nameof(GetIncidentById),
                new { id },
                result.Value),
            _ => BadRequest(result.Errors)
        };
    }



    [HttpGet("paged")]
    [ProducesResponseType(typeof(List<IncidentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPagedIncidents(
          [FromQuery] SupportStatus? supportStatus,
          [FromQuery] UserStatus? userStatus,
          [FromQuery] Module? module,
          [FromQuery] Priority? priority,
          [FromQuery] Guid? assignedToId,
          [FromQuery] DateTime? fromDate,
          [FromQuery] DateTime? toDate,
          [FromQuery] int? pageNumber,
          [FromQuery] int? pageSize)
    {
        var query = new PagedIncidentsQuery(
          supportStatus,
          userStatus,
          module,
          priority,
          assignedToId,
          fromDate,
          toDate,
          pageNumber ?? 1,
          pageSize ?? 5);

        var result = await mediator.Send(query);

        return result.IsSuccess
          ? Ok(result.Value)
          : BadRequest(result.Errors);
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(List<IncidentListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetListIncidents()
    {
        var result = await mediator.Send(new ListIncidentsQuery());

        return Ok(result.Value);
    }


    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateIncident(Guid id, [FromBody] UpdateIncidentDto dto)
    {
        var command = new UpdateIncidentCommand(
            id,
            dto.Suggestion,
            dto.UserStatus,
            dto.SupportStatus,
            dto.AssignedToId,
            dto.DeliveryDate
        );

        var validation = await updateIncidentCommandValidator.ValidateAsync(command);

        if (!validation.IsValid)
        {
            var errors = validation.Errors.Select(e => e.ErrorMessage);
            return BadRequest(Result.Invalid(new List<ValidationError>(errors.Select(e => new ValidationError(e)))));
        }

        var result = await mediator.Send(command);

        if (result.Status == ResultStatus.Unauthorized)
        {
            return Forbid();
        }

        if (result.Status == ResultStatus.NotFound)
        {
            return NotFound(result.Errors);
        }

        if (result.Status == ResultStatus.Invalid)
        {
            return BadRequest(result.ValidationErrors);
        }

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
    }


    [HttpPut("{id:guid}/attachments")]
    [RequestSizeLimit(FileConstants.MaxUploadSize)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateIncidentAttachments(Guid id, [FromForm] AddAttachmentsDto addAttachmentsDto)
    {
        using var scope = logger.BeginScope("UpdateIncidentAttachments: {CorrelationId}", Guid.NewGuid());
        logger.LogInformation("Processing attachment update for incident {IncidentId}", id);

        var command = new UpdateIncidentAttachmentsCommand(id, addAttachmentsDto.Files);
        var validationResult = await updateIncidentAttachmentsCommandValidator.ValidateAsync(command);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            logger.LogWarning("Validation failed: {Errors}", string.Join("; ", errors));
            return BadRequest(Result.Invalid(errors.Select(e => new ValidationError(e)).ToList()));
        }

        var result = await mediator.Send(command);

        return result.Status switch
        {
            ResultStatus.Unauthorized => Forbid(),
            ResultStatus.NotFound => NotFound(result.Errors),
            ResultStatus.Invalid => BadRequest(result.ValidationErrors),
            ResultStatus.Ok => NoContent(),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [HttpPost("{id:guid}/remove-attachments")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveIncidentAttachments(Guid id, [FromBody] RemoveAttachmentsDto dto)
    {
        using var scope = logger.BeginScope("RemoveIncidentAttachments: {CorrelationId}", Guid.NewGuid());
        logger.LogInformation("Processing attachment removal for incident {IncidentId}", id);

        var command = new RemoveIncidentAttachmentsCommand(id, dto.AttachmentIds);

        var validationResult = await removeIncidentAttachmentsCommandValidator.ValidateAsync(command);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            logger.LogWarning("Validation failed: {Errors}", string.Join("; ", errors));
            return BadRequest(Result.Invalid(errors.Select(e => new ValidationError(e)).ToList()));
        }

        var result = await mediator.Send(command);

        return result.Status switch
        {
            ResultStatus.Unauthorized => Forbid(),
            ResultStatus.NotFound => NotFound(result.Errors),
            ResultStatus.Invalid => BadRequest(result.ValidationErrors),
            ResultStatus.Ok => NoContent(),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [HttpPut("{id:guid}/comments")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateIncidentComments(Guid id, [FromBody] AddCommentsDto dto)
    {
        using var scope = logger.BeginScope("UpdateIncidentComments: {CorrelationId}", Guid.NewGuid());
        logger.LogInformation("Processing comment update for incident {IncidentId}", id);

        var command = new AddIncidentCommentsCommand(id, dto.Comments);
        var validationResult = await addIncidentCommentCommandValidator.ValidateAsync(command);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            logger.LogWarning("Validation failed: {Errors}", string.Join("; ", errors));
            return BadRequest(Result.Invalid(errors.Select(e => new ValidationError(e)).ToList()));
        }

        var result = await mediator.Send(command);

        return result.Status switch
        {
            ResultStatus.Unauthorized => Forbid(),
            ResultStatus.NotFound => NotFound(result.Errors),
            ResultStatus.Invalid => BadRequest(result.ValidationErrors),
            ResultStatus.Ok => NoContent(),
            _ => StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };
    }

    [HttpGet("{id:guid}/comments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetIncidentComments(Guid id)
    {
        using var scope = logger.BeginScope("GetIncidentComments: {CorrelationId}", Guid.NewGuid());
        logger.LogInformation("Processing comment Get for incident {IncidentId}", id);

        var query = new ListCommentsQuery(id);

        var result = await mediator.Send(query);

        return Ok(result.Value);
    }
}



