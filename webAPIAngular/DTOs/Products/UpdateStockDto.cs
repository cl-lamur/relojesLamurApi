namespace RelojesLamur.API.DTOs.Products;

public class UpdateStockDto
{
    public bool InStock { get; set; }
}

public class StockResponseDto
{
    public int Id { get; set; }
    public bool InStock { get; set; }
}
