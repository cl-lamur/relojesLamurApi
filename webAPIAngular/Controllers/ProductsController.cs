using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RelojesLamur.API.Common;
using RelojesLamur.API.DTOs.Common;
using RelojesLamur.API.DTOs.Products;
using RelojesLamur.API.Services;
using RelojesLamur.API.Services.Interfaces;

namespace RelojesLamur.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProductsController(IProductService productService, FirebaseStorageService firebaseStorageService) : ControllerBase
{
    // GET /api/products
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] string?  category,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] string?  search,
        [FromQuery] int      page     = 1,
        [FromQuery] int      pageSize = 12)
    {
        var result = await productService.GetAllAsync(category, minPrice, maxPrice, search, page, pageSize);
        return Ok(ApiResponse<PagedResultDto<ProductDto>>.Ok(result));
    }

    // GET /api/products/{id}
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await productService.GetByIdAsync(id);
        if (result is null)
            return NotFound(ApiResponse.Fail($"Producto {id} no encontrado."));

        return Ok(ApiResponse<ProductDetailDto>.Ok(result));
    }

    // POST /api/products/upload
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UploadImage([FromForm] UploadImageDto dto)
    {
        if (dto.File is null)
            return BadRequest(ApiResponse.Fail("El archivo de imagen es obligatorio."));

        try
        {
            var url = await firebaseStorageService.UploadAsync(dto.File);
            return Ok(ApiResponse<object>.Ok(new { url }, "Imagen subida correctamente."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
        catch (Exception)
        {
            return StatusCode(500, ApiResponse.Fail("Error al subir la imagen."));
        }
    }

    // POST /api/products
    [HttpPost]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var result = await productService.CreateAsync(dto);
        return StatusCode(201, ApiResponse<ProductDetailDto>.Ok(result, "Producto creado correctamente."));
    }

    // PUT /api/products/{id}
    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProductDto dto)
    {
        var result = await productService.UpdateAsync(id, dto);
        return Ok(ApiResponse<ProductDetailDto>.Ok(result, "Producto actualizado correctamente."));
    }

    // PATCH /api/products/{id}/stock
    [HttpPatch("{id:int}/stock")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto dto)
    {
        var result = await productService.UpdateStockAsync(id, dto.InStock);
        return Ok(ApiResponse<StockResponseDto>.Ok(result));
    }

    // DELETE /api/products/{id}
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await productService.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Producto eliminado."));
    }
}
