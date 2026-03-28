using RelojesLamur.API.DTOs.Common;
using RelojesLamur.API.DTOs.Users;

namespace RelojesLamur.API.Services.Interfaces;

public interface IUserService
{
    Task<PagedResultDto<UserDto>> GetAllAsync(int page, int pageSize, string? search);
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<RoleResponseDto> UpdateRoleAsync(Guid id, string role);
    Task DeleteAsync(Guid id);
}
