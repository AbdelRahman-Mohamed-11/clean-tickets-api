using FluentValidation;
using TicketingSystem.Application.Users.Register;
using TicketingSystem.Core.Constatns;

namespace TicketingSystem.Application.Users.Login;

public class LoginUserCommandValidator
    : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
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
