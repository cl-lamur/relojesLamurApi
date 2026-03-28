using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RelojesLamur.API.Common;
using RelojesLamur.API.DTOs.Auth;
using RelojesLamur.API.Services.Interfaces;

namespace RelojesLamur.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var result = await authService.RegisterAsync(dto);
        return StatusCode(201, ApiResponse<RegisterResponseDto>.Ok(result, "Usuario registrado correctamente."));
    }

    // POST /api/auth/login  ? rate limiting: 10 req/min por IP
    [HttpPost("login")]
    [EnableRateLimiting("LoginPolicy")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var result = await authService.LoginAsync(dto);
        return Ok(ApiResponse<LoginResponseDto>.Ok(result));
    }

    // GET /api/auth/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var result = await authService.GetCurrentUserAsync(userId);
        return Ok(ApiResponse<UserInfoDto>.Ok(result));
    }
}
