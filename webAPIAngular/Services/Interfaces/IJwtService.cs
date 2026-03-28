using RelojesLamur.API.Entities;

namespace RelojesLamur.API.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    DateTime GetExpirationDate();
}
