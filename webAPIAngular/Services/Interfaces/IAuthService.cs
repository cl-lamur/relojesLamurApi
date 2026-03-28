using RelojesLamur.API.DTOs.Auth;

namespace RelojesLamur.API.Services.Interfaces;

public interface IAuthService
{
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto dto);
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
    Task<UserInfoDto> GetCurrentUserAsync(Guid userId);
}
