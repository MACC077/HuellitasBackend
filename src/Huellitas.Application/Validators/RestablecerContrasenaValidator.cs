using FluentValidation;
using Huellitas.Application.DTOs.Usuarios;

namespace Huellitas.Application.Validators;

public class RestablecerContrasenaValidator : AbstractValidator<RestablecerContrasenaDTO>
{
    public RestablecerContrasenaValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("El token es obligatorio");

        RuleFor(x => x.NuevaContrasena)
            .NotEmpty().WithMessage("La nueva contraseña es obligatoria")
            .MinimumLength(8).WithMessage("Debe tener mínimo 8 caracteres")
            .MaximumLength(100);
    }
}