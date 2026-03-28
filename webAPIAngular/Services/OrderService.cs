using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RelojesLamur.API.Data;
using RelojesLamur.API.DTOs.Common;
using RelojesLamur.API.DTOs.Orders;
using RelojesLamur.API.Entities;
using RelojesLamur.API.Services.Interfaces;

namespace RelojesLamur.API.Services;

public class OrderService(AppDbContext context, IMapper mapper) : IOrderService
{
    private const decimal TaxRate = 0.21m;

    public async Task<OrderDetailDto> CreateOrderAsync(Guid userId, CreateOrderDto dto)
    {
        if (dto.Items.Count == 0)
            throw new InvalidOperationException("El pedido debe tener al menos un producto.");

        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync();

        var orderItems = new List<OrderItem>();
        decimal subtotal = 0;

        foreach (var item in dto.Items)
        {
            var product = products.FirstOrDefault(p => p.Id == item.ProductId)
                ?? throw new KeyNotFoundException($"Producto {item.ProductId} no encontrado.");

            if (!product.InStock)
                throw new InvalidOperationException($"El producto '{product.Name}' no tiene stock.");

            if (item.Quantity <= 0)
                throw new InvalidOperationException($"La cantidad debe ser mayor a 0 para '{product.Name}'.");

            var lineTotal = product.Price * item.Quantity;
            subtotal += lineTotal;

            orderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            });
        }

        var tax = Math.Round(subtotal * TaxRate, 2);
        var total = subtotal + tax;

        var order = new Order
        {
            UserId = userId,
            Subtotal = subtotal,
            Tax = tax,
            Total = total,
            Status = "pending",
            CreatedAt = DateTime.UtcNow,
            Items = orderItems
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return await LoadOrderDetailAsync(order.Id);
    }

    public async Task<PagedResultDto<OrderDto>> GetUserOrdersAsync(Guid userId, int page, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(page, 1);

        var query = context.Orders.Where(o => o.UserId == userId);
        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<OrderDto>
        {
            Items = mapper.Map<List<OrderDto>>(items),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<OrderDetailDto?> GetOrderByIdAsync(Guid orderId, Guid userId, bool isAdmin)
    {
        // IgnoreQueryFilters: muestra datos históricos aunque el producto esté soft-deleted
        var order = await context.Orders
            .IgnoreQueryFilters()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId && !o.User.IsDeleted);

        if (order is null) return null;

        if (!isAdmin && order.UserId != userId)
            throw new UnauthorizedAccessException("No tienes permiso para ver este pedido.");

        return mapper.Map<OrderDetailDto>(order);
    }

    public async Task<CancelOrderResponseDto> CancelOrderAsync(Guid orderId, Guid userId)
    {
        var order = await context.Orders.FindAsync(orderId)
            ?? throw new KeyNotFoundException("Pedido no encontrado.");

        if (order.UserId != userId)
            throw new UnauthorizedAccessException("No tienes permiso para cancelar este pedido.");

        if (order.Status != "pending")
            throw new InvalidOperationException("Solo se pueden cancelar pedidos en estado 'pending'.");

        order.Status = "cancelled";
        await context.SaveChangesAsync();

        return new CancelOrderResponseDto { Id = order.Id, Status = order.Status };
    }

    public async Task<PagedResultDto<AdminOrderDto>> GetAllOrdersAsync(int page, int pageSize, string? status)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(page, 1);

        // IgnoreQueryFilters: el admin debe ver pedidos de usuarios soft-deleted
        var query = context.Orders
            .IgnoreQueryFilters()
            .Include(o => o.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(o => o.Status == status.ToLower().Trim());

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<AdminOrderDto>
        {
            Items = mapper.Map<List<AdminOrderDto>>(items),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    private async Task<OrderDetailDto> LoadOrderDetailAsync(Guid orderId)
    {
        // IgnoreQueryFilters: el ítem recién creado debe cargar el producto aunque esté soft-deleted
        var order = await context.Orders
            .IgnoreQueryFilters()
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstAsync(o => o.Id == orderId);

        return mapper.Map<OrderDetailDto>(order);
    }
}
