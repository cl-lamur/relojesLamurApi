using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RelojesLamur.API.Data;
using RelojesLamur.API.DTOs.Common;
using RelojesLamur.API.DTOs.Products;
using RelojesLamur.API.Entities;
using RelojesLamur.API.Services.Interfaces;

namespace RelojesLamur.API.Services;

public class ProductService(
    AppDbContext context,
    IMapper mapper,
    IWebHostEnvironment env,
    IHttpContextAccessor http) : IProductService
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
        var imageUrl = IsBase64Image(dto.ImageUrl)
            ? await SaveBase64ImageAsync(dto.ImageUrl)
            : dto.ImageUrl;

        var product = new Product
        {
            Name        = dto.Name,
            Description = dto.Description,
            Price       = dto.Price,
            Category    = dto.Category.ToLower().Trim(),
            ImageUrl    = imageUrl,
            InStock     = dto.InStock,
            CreatedAt   = DateTime.UtcNow
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

        product.Name        = dto.Name;
        product.Description = dto.Description;
        product.Price       = dto.Price;
        product.Category    = dto.Category.ToLower().Trim();
        product.ImageUrl    = IsBase64Image(dto.ImageUrl)
                                  ? await SaveBase64ImageAsync(dto.ImageUrl)
                                  : dto.ImageUrl;
        product.InStock     = dto.InStock;
        product.UpdatedAt   = DateTime.UtcNow;

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

    // ?? Helpers de imagen ????????????????????????????????????????????????????

    private static bool IsBase64Image(string? url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        if (url.StartsWith("data:image/")) return true;
        return url.StartsWith("iVBORw0K")   // PNG
            || url.StartsWith("/9j/")        // JPEG
            || url.StartsWith("R0lGOD")     // GIF
            || url.StartsWith("UklGR");     // WebP
    }

    private async Task<string> SaveBase64ImageAsync(string imageData)
    {
        byte[] bytes;
        string ext;

        if (imageData.StartsWith("data:image/"))
        {
            // Formato: "data:image/png;base64,iVBORw0K..."
            var semicolon = imageData.IndexOf(';');
            var format    = imageData[11..semicolon];               // "png", "jpeg", "webp"
            ext           = format.Equals("jpeg", StringComparison.OrdinalIgnoreCase) ? "jpg" : format;
            var base64    = imageData[(imageData.IndexOf(',') + 1)..];
            bytes         = Convert.FromBase64String(base64);
        }
        else
        {
            // Base64 raw sin prefijo
            bytes = Convert.FromBase64String(imageData);
            ext   = DetectExtension(bytes);
        }

        // Carpeta destino: wwwroot/ProductImages/
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var folder  = Path.Combine(webRoot, "ProductImages");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}.{ext}";
        await File.WriteAllBytesAsync(Path.Combine(folder, fileName), bytes);

        // Construir URL pública usando la petición actual
        var req     = http.HttpContext?.Request;
        var baseUrl = req is not null
            ? $"{req.Scheme}://{req.Host}"
            : "https://localhost:44343";

        return $"{baseUrl}/ProductImages/{fileName}";
    }

    private static string DetectExtension(byte[] bytes) => bytes switch
    {
        _ when bytes.Length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50 => "png",
        _ when bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 => "jpg",
        _ when bytes.Length >= 6 && bytes[0] == 0x47 && bytes[1] == 0x49 => "gif",
        _ when bytes.Length >= 4 && bytes[0] == 0x52 && bytes[1] == 0x49 => "webp",
        _                                                                  => "png"
    };
}
