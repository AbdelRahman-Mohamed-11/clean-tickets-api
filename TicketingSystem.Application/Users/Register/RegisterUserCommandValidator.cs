using FluentValidation;
using TicketingSystem.Core.Entities.Identity.Constatns;

namespace TicketingSystem.Application.Users.Register;

public class RegisterUserCommandValidator
    : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage(ValidationConstants.UserNameRequired)
        .MinimumLength(ValidationConstants.UserNameMinLen)
                .WithMessage(string.Format(ValidationConstants.UserNameMinLength, ValidationConstants.UserNameMinLen));
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(ValidationConstants.EmailRequired)
            .EmailAddress().WithMessage(ValidationConstants.EmailInvalid);
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationConstants.PasswordRequired)
        .MinimumLength(ValidationConstants.PasswordMinLen)
                .WithMessage(string.Format(ValidationConstants.PasswordMinLength, ValidationConstants.PasswordMinLen))
        .Matches("[A-Z]")
                .WithMessage(ValidationConstants.PasswordRequireUppercase)
        .Matches("[a-z]")
                .WithMessage(ValidationConstants.PasswordRequireLowercase)
        .Matches(@"\d")
                .WithMessage(ValidationConstants.PasswordRequireDigit)
            .Matches(@"[^a-zA-Z0-9]")
                .WithMessage(ValidationConstants.PasswordRequireNonAlphanumeric);
    }
}
