using Microsoft.EntityFrameworkCore;
using RelojesLamur.API.Data;
using RelojesLamur.API.DTOs.Auth;
using RelojesLamur.API.Entities;
using RelojesLamur.API.Services.Interfaces;

namespace RelojesLamur.API.Services;

public class AuthService(AppDbContext context, IJwtService jwtService) : IAuthService
{
    public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        if (await context.Users.AnyAsync(u => u.Email == dto.Email.ToLower().Trim()))
            throw new InvalidOperationException("El email ya está registrado.");

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, 12),
            Role = "user",
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new RegisterResponseDto
        {
            Token = jwtService.GenerateToken(user),
            User = ToInfo(user)
        };
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower().Trim());

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas.");

        return new LoginResponseDto
        {
            Token = jwtService.GenerateToken(user),
            ExpiresAt = jwtService.GetExpirationDate(),
            User = ToInfo(user)
        };
    }

    public async Task<UserInfoDto> GetCurrentUserAsync(Guid userId)
    {
        var user = await context.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");
        return ToInfo(user);
    }

    private static UserInfoDto ToInfo(User u) => new()
    { Id = u.Id, Name = u.Name, Email = u.Email, Role = u.Role };
}
