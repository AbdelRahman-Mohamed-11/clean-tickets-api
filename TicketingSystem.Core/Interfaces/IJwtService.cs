using TicketingSystem.Core.Dtos.Identity;

namespace TicketingSystem.Infrastructure.Interfaces;

public interface IJwtService
{
    Task<AuthResponseDto> RefreshAsync(UserRefreshToken userRefreshToken);
    Task<AuthResponseDto> GenerateJWTokenAsync(string userId);
    string GenerateRefreshToken();
}
