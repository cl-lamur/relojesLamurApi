using RelojesLamur.API.DTOs.Common;
using RelojesLamur.API.DTOs.Orders;

namespace RelojesLamur.API.Services.Interfaces;

public interface IOrderService
{
    Task<OrderDetailDto> CreateOrderAsync(Guid userId, CreateOrderDto dto);
    Task<PagedResultDto<OrderDto>> GetUserOrdersAsync(Guid userId, int page, int pageSize);
    Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId, Guid userId, bool isAdmin);
    Task<CancelOrderResponseDto> CancelOrderAsync(Guid orderId, Guid userId);
    Task<PagedResultDto<AdminOrderDto>> GetAllOrdersAsync(int page, int pageSize, string? status);
}
