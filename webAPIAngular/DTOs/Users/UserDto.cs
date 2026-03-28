namespace RelojesLamur.API.DTOs.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdateRoleDto
{
    public string Role { get; set; } = string.Empty;
}

public class RoleResponseDto
{
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;
}
