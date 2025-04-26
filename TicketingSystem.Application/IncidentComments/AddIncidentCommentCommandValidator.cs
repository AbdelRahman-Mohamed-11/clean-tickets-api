using FluentValidation;

namespace TicketingSystem.Application.IncidentComments
{
    public class AddIncidentCommentCommandValidator
        : AbstractValidator<AddIncidentCommentCommand>
    {
        public AddIncidentCommentCommandValidator()
        {
            RuleFor(c => c.Text)
                .NotEmpty().WithMessage("Comment text is required.")
                .MaximumLength(500).WithMessage("Comment cannot exceed 500 characters.");
        }
    }
}
