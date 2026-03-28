namespace RelojesLamur.API.Entities;

public class ProductFeatures
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? Material { get; set; }
    public string? Mechanism { get; set; }
    public string? WaterResistance { get; set; }

    public Product Product { get; set; } = null!;
}
