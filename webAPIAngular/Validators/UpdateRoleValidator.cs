using FluentValidation;
using RelojesLamur.API.DTOs.Users;

namespace RelojesLamur.API.Validators;

public class UpdateRoleValidator : AbstractValidator<UpdateRoleDto>
{
    private static readonly string[] ValidRoles = ["admin", "user"];

    public UpdateRoleValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(r => ValidRoles.Contains(r.ToLower().Trim()))
            .WithMessage("El rol debe ser 'admin' o 'user'.");
    }
}
