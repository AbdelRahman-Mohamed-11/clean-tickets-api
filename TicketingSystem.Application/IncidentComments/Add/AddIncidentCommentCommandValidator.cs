using FluentValidation;

namespace TicketingSystem.Application.IncidentComments.Add
{
    public class AddIncidentCommentCommandValidator
        : AbstractValidator<AddIncidentCommentsCommand>
    {
        public AddIncidentCommentCommandValidator()
        {
            RuleFor(c => c.IncidentId)
                .NotEmpty().WithMessage("Incident ID is required.")
                .NotEqual(Guid.Empty).WithMessage("Incident ID cannot be empty.");

            RuleFor(c => c.Comments)
                .NotEmpty().WithMessage("Comment text is required.");
        }
    }
}
