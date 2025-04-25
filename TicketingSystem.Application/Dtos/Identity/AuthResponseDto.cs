namespace TicketingSystem.Application.Dtos.Identity
{
    public class AuthResponseDto
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public string? UserName { get; set; }
        public IReadOnlyList<string> Roles { get; set; } = []; // will search
        public DateTime? ExpireIn { get; set; }
    }
}
