using FluentValidation;
using TicketingSystem.Core.Constatns;

namespace TicketingSystem.Application.incidents.Create
{
    public class CreateIncidentCommandValidator
        : AbstractValidator<CreateIncidentCommand>
    {
        public CreateIncidentCommandValidator()
        {
            RuleFor(x => x.UrlOrFormName)
                .NotEmpty().WithMessage("URL/Form Name is required.")
                .MaximumLength(ValidationConstants.UrlMaxLength)
                    .WithMessage($"URL/Form Name must not exceed {ValidationConstants.UrlMaxLength} characters.");

            RuleFor(x => x.IsRecurring)
                .Must((cmd, isRecurring) => !isRecurring || cmd.RecurringCallId.HasValue)
                .WithMessage("RecurringCallId must be specified when IsRecurring is true.");

            RuleFor(x => x.RecurringCallId)
                .NotEmpty()
                .When(x => x.IsRecurring)
                .WithMessage("RecurringCallId is required for recurring incidents.");

            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage("Subject is required.")
                .MaximumLength(ValidationConstants.SubjectMaxLength)
                    .WithMessage($"Subject must not exceed {ValidationConstants.SubjectMaxLength} characters.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.");

            RuleFor(x => x.Suggestion)
                .MaximumLength(ValidationConstants.SuggestionMaxLength)
                    .WithMessage($"Suggestion must not exceed {ValidationConstants.SuggestionMaxLength} characters.");
        }
    }
}
