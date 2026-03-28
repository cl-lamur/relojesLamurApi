namespace RelojesLamur.API.DTOs.Orders;

public class OrderDetailDto : OrderDto
{
    public List<OrderItemDetailDto> Items { get; set; } = [];
}

public class OrderItemDetailDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductImageUrl { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;   // calculado: front lo usa directo
}

public class CancelOrderResponseDto
{
    public Guid Id { get; set; }
    public string Status { get; set; } = "cancelled";
}
