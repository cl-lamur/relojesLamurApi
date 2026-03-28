using RelojesLamur.API.DTOs.Common;
using RelojesLamur.API.DTOs.Products;

namespace RelojesLamur.API.Services.Interfaces;

public interface IProductService
{
    Task<PagedResultDto<ProductDto>> GetAllAsync(string? category, decimal? minPrice, decimal? maxPrice, string? search, int page, int pageSize);
    Task<ProductDetailDto?> GetByIdAsync(int id);
    Task<ProductDetailDto> CreateAsync(CreateProductDto dto);
    Task<ProductDetailDto> UpdateAsync(int id, CreateProductDto dto);
    Task<StockResponseDto> UpdateStockAsync(int id, bool inStock);
    Task DeleteAsync(int id);
}
