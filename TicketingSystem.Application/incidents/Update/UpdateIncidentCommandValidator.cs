using FluentValidation;
using TicketingSystem.Application.incidents.Update;

namespace TicketingSystem.Application.Incidents.Update
{
    public class UpdateIncidentCommandValidator : AbstractValidator<UpdateIncidentCommand>
    {
        public UpdateIncidentCommandValidator()
        {
            RuleFor(cmd => cmd.Id)
                .NotEmpty()
                .WithMessage("Incident Id is required.");

            RuleFor(cmd => cmd)
               .Must(cmd =>
                   cmd.Suggestion != null ||
                   cmd.UserStatus != null ||
                   cmd.SupportStatus != null ||
                   cmd.AssignedToId != null ||
                   cmd.DeliveryDate != null)
           .WithMessage("At least one field must be provided for update.");
        }
    }
}