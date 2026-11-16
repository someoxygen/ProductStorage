using ProductStorageAPI.DTOs.Auth;

namespace ProductStorageAPI.Abstractions
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default);
        Task<AuthResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);
    }
}
