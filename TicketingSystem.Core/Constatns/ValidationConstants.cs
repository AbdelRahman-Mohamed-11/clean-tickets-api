namespace TicketingSystem.Core.Constatns;

public class ValidationConstants
{
    public const int UserNameMinLen = 3;
    public const int PasswordMinLen = 6;
    public const int UrlMaxLength = 200;
    public const int SubjectMaxLength = 200;
    public const int SuggestionMaxLength = 500;

    public const string UserNameRequired = "Username is required.";
    public const string UserNameMinLength = "Username must be at least {0} characters long.";

    public const string EmailRequired = "Email is required.";
    public const string EmailInvalid = "Email must be a valid email address.";

    public const string PasswordRequired = "Password is required.";
    public const string PasswordMinLength = "Password must be at least {0} characters long.";
    public const string PasswordRequireUppercase = "Password must contain at least one uppercase letter.";
    public const string PasswordRequireLowercase = "Password must contain at least one lowercase letter.";
    public const string PasswordRequireDigit = "Password must contain at least one digit.";
    public const string PasswordRequireNonAlphanumeric = "Password must contain at least one non-alphanumeric character.";
}


