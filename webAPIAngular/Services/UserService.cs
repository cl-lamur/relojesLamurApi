using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RelojesLamur.API.Data;
using RelojesLamur.API.DTOs.Common;
using RelojesLamur.API.DTOs.Users;
using RelojesLamur.API.Services.Interfaces;

namespace RelojesLamur.API.Services;

public class UserService(AppDbContext context, IMapper mapper) : IUserService
{
    public async Task<PagedResultDto<UserDto>> GetAllAsync(int page, int pageSize, string? search)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(page, 1);

        var query = context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(u =>
                EF.Functions.Like(u.Name, $"%{term}%") ||
                EF.Functions.Like(u.Email, $"%{term}%"));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<UserDto>
        {
            Items = mapper.Map<List<UserDto>>(items),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await context.Users.FindAsync(id);
        return user is null ? null : mapper.Map<UserDto>(user);
    }

    public async Task<RoleResponseDto> UpdateRoleAsync(Guid id, string role)
    {
        var user = await context.Users.FindAsync(id)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        user.Role = role.ToLower().Trim();
        await context.SaveChangesAsync();

        return new RoleResponseDto { Id = user.Id, Role = user.Role };
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await context.Users.FindAsync(id)
            ?? throw new KeyNotFoundException("Usuario no encontrado.");

        user.IsDeleted = true;
        await context.SaveChangesAsync();
    }
}
