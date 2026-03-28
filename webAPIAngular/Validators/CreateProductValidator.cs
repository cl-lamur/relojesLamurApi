using FluentValidation;
using RelojesLamur.API.DTOs.Products;

namespace RelojesLamur.API.Validators;

public class CreateProductValidator : AbstractValidator<CreateProductDto>
{
    private static readonly string[] ValidCategories = ["hombre", "mujer"];

    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().MaximumLength(150);

        RuleFor(x => x.Description)
            .NotEmpty().MaximumLength(1000);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a 0.");

        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(c => ValidCategories.Contains(c.ToLower().Trim()))
            .WithMessage("La categoría debe ser 'hombre' o 'mujer'.");

        RuleFor(x => x.ImageUrl)
            .NotEmpty().MaximumLength(500);
    }
}
