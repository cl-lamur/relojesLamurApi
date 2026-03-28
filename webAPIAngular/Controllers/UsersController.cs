using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelojesLamur.API.Common;
using RelojesLamur.API.DTOs.Common;
using RelojesLamur.API.DTOs.Users;
using RelojesLamur.API.Services.Interfaces;

namespace RelojesLamur.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "admin")]
public class UsersController(IUserService userService) : ControllerBase
{
    // GET /api/users
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 10,
        [FromQuery] string? search   = null)
    {
        var result = await userService.GetAllAsync(page, pageSize, search);
        return Ok(ApiResponse<PagedResultDto<UserDto>>.Ok(result));
    }

    // GET /api/users/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await userService.GetByIdAsync(id);
        if (result is null)
            return NotFound(ApiResponse.Fail("Usuario no encontrado."));

        return Ok(ApiResponse<UserDto>.Ok(result));
    }

    // PATCH /api/users/{id}/role
    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleDto dto)
    {
        var result = await userService.UpdateRoleAsync(id, dto.Role);
        return Ok(ApiResponse<RoleResponseDto>.Ok(result));
    }

    // DELETE /api/users/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await userService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Usuario eliminado."));
    }
}
