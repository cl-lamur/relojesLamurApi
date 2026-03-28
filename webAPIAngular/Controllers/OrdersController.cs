using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelojesLamur.API.Common;
using RelojesLamur.API.DTOs.Common;
using RelojesLamur.API.DTOs.Orders;
using RelojesLamur.API.Services.Interfaces;

namespace RelojesLamur.API.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    // GET /api/orders/admin/all  ? debe ir ANTES de {id:guid} para evitar colisión
    [HttpGet("admin/all")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 10,
        [FromQuery] string? status   = null)
    {
        var result = await orderService.GetAllOrdersAsync(page, pageSize, status);
        return Ok(ApiResponse<PagedResultDto<AdminOrderDto>>.Ok(result));
    }

    // POST /api/orders
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var userId = GetUserId();
        var result = await orderService.CreateOrderAsync(userId, dto);
        return StatusCode(201, ApiResponse<OrderDetailDto>.Ok(result, "Pedido creado correctamente."));
    }

    // GET /api/orders
    [HttpGet]
    public async Task<IActionResult> GetMine(
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetUserId();
        var result = await orderService.GetUserOrdersAsync(userId, page, pageSize);
        return Ok(ApiResponse<PagedResultDto<OrderDto>>.Ok(result));
    }

    // GET /api/orders/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId  = GetUserId();
        var isAdmin = User.IsInRole("admin");

        var result = await orderService.GetOrderByIdAsync(id, userId, isAdmin);
        if (result is null)
            return NotFound(ApiResponse.Fail("Pedido no encontrado."));

        return Ok(ApiResponse<OrderDetailDto>.Ok(result));
    }

    // PATCH /api/orders/{id}/cancel
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetUserId();
        var result = await orderService.CancelOrderAsync(id, userId);
        return Ok(ApiResponse<CancelOrderResponseDto>.Ok(result));
    }

    // ?? Helper ???????????????????????????????????????????????
    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
}
