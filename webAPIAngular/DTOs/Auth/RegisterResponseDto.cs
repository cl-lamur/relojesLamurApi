namespace RelojesLamur.API.DTOs.Auth;

public class RegisterResponseDto
{
    public string Token { get; set; } = string.Empty;
    public UserInfoDto User { get; set; } = null!;
}
