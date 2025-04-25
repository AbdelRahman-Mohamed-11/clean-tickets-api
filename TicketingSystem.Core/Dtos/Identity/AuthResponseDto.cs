namespace TicketingSystem.Core.Dtos.Identity;

public class AuthResponseDto
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public string? UserName { get; set; }
    public IList<string> Roles { get; set; } = [];
    public DateTime? ExpireIn { get; set; }
}
