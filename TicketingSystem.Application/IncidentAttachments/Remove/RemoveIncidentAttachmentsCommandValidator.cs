using FluentValidation;

namespace TicketingSystem.Application.IncidentAttachments.Remove;

public class RemoveIncidentAttachmentsCommandValidator : AbstractValidator<RemoveIncidentAttachmentsCommand>
{
    public RemoveIncidentAttachmentsCommandValidator()
    {
        RuleFor(x => x.IncidentId)
            .NotEmpty().WithMessage("Incident ID is required");
    }
}