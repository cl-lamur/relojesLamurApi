using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RelojesLamur.API.Data;
using RelojesLamur.API.DTOs.Common;
using RelojesLamur.API.DTOs.Products;
using RelojesLamur.API.Entities;
using RelojesLamur.API.Services.Interfaces;

namespace RelojesLamur.API.Services;

public class ProductService(AppDbContext context, IMapper mapper) : IProductService
{
    public async Task<PagedResultDto<ProductDto>> GetAllAsync(
        string? category, decimal? minPrice, decimal? maxPrice,
        string? search, int page, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(page, 1);

        var query = context.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category.ToLower().Trim());

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p =>
                EF.Functions.Like(p.Name, $"%{term}%") ||
                EF.Functions.Like(p.Description, $"%{term}%"));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<ProductDto>
        {
            Items = mapper.Map<List<ProductDto>>(items),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductDetailDto?> GetByIdAsync(int id)
    {
        var product = await context.Products
            .Include(p => p.Features)
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? null : mapper.Map<ProductDetailDto>(product);
    }

    public async Task<ProductDetailDto> CreateAsync(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Category = dto.Category.ToLower().Trim(),
            ImageUrl = dto.ImageUrl,
            InStock = dto.InStock,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Features is not null)
        {
            product.Features = new ProductFeatures
            {
                Material = dto.Features.Material,
                Mechanism = dto.Features.Mechanism,
                WaterResistance = dto.Features.WaterResistance
            };
        }

        context.Products.Add(product);
        await context.SaveChangesAsync();

        return mapper.Map<ProductDetailDto>(product);
    }

    public async Task<ProductDetailDto> UpdateAsync(int id, CreateProductDto dto)
    {
        var product = await context.Products
            .Include(p => p.Features)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Producto {id} no encontrado.");

        product.Name = dto.Name;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.Category = dto.Category.ToLower().Trim();
        product.ImageUrl = dto.ImageUrl;
        product.InStock = dto.InStock;
        product.UpdatedAt = DateTime.UtcNow;

        if (dto.Features is not null)
        {
            if (product.Features is null)
                product.Features = new ProductFeatures { ProductId = product.Id };

            product.Features.Material = dto.Features.Material;
            product.Features.Mechanism = dto.Features.Mechanism;
            product.Features.WaterResistance = dto.Features.WaterResistance;
        }

        await context.SaveChangesAsync();
        return mapper.Map<ProductDetailDto>(product);
    }

    public async Task<StockResponseDto> UpdateStockAsync(int id, bool inStock)
    {
        var product = await context.Products.FindAsync(id)
            ?? throw new KeyNotFoundException($"Producto {id} no encontrado.");

        product.InStock = inStock;
        product.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();

        return new StockResponseDto { Id = product.Id, InStock = product.InStock };
    }

    public async Task DeleteAsync(int id)
    {
        var product = await context.Products.FindAsync(id)
            ?? throw new KeyNotFoundException($"Producto {id} no encontrado.");

        product.IsDeleted = true;
        product.UpdatedAt = DateTime.UtcNow;
        await context.SaveChangesAsync();
    }
}
