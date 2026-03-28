namespace RelojesLamur.API.DTOs.Products;

public class ProductDetailDto : ProductDto
{
    public DateTime? UpdatedAt { get; set; }
    public ProductFeaturesDto? Features { get; set; }
}
