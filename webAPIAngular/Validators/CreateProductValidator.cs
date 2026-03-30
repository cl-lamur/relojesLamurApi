using FluentValidation;
using RelojesLamur.API.DTOs.Products;

namespace RelojesLamur.API.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    private static readonly string[] ValidCategories = new[] { "hombre", "mujer" };

    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("El nombre del producto es obligatorio.")
            .MaximumLength(150).WithMessage("El nombre no puede superar los 150 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción del producto es obligatoria.")
            .MaximumLength(1000).WithMessage("La descripción no puede superar los 1000 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser un número mayor a 0.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("La categoría es obligatoria.")
            .Must(c => ValidCategories.Contains(c.ToLower().Trim()))
            .WithMessage("La categoría debe ser exactamente 'hombre' o 'mujer' (en minúsculas).");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().WithMessage("La URL de la imagen es obligatoria.")
            .Must(BeValidImageSource)
            .WithMessage("La imageUrl debe ser una URL válida (https://...) o una imagen en base64.");
    }

    private static bool BeValidImageSource(string? url)
    {
        if (string.IsNullOrEmpty(url)) return false;

        // URL válida
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
            (uri.Scheme == "https" || uri.Scheme == "http"))
            return true;

        // Base64 con prefijo data URI
        if (url.StartsWith("data:image/")) return true;

        // Base64 raw — firmas mágicas de PNG, JPEG, GIF, WebP
        return url.StartsWith("iVBORw0K")  // PNG
            || url.StartsWith("/9j/")       // JPEG
            || url.StartsWith("R0lGOD")    // GIF
            || url.StartsWith("UklGR");    // WebP
    }
}
