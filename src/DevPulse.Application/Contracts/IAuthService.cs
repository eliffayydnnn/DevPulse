using DevPulse.Application.DTOs;

namespace DevPulse.Application.Contracts;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
