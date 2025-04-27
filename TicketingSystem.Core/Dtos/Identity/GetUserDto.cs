namespace TicketingSystem.Core.Dtos.Identity;

public record GetUserDto(Guid Id, string UserName, string Email)
{
    public IList<string> Roles { get; init; } = [];
}