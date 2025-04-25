namespace TicketingSystem.Application.Dtos.Identity;

public class UserRefreshToken
{
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}
