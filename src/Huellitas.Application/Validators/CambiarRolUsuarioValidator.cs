using FluentValidation;
using Huellitas.Application.DTOs.Administracion;

namespace Huellitas.Application.Validators;

public class CambiarRolUsuarioValidator : AbstractValidator<CambiarRolUsuarioDTO>
{
    public CambiarRolUsuarioValidator()
    {
        RuleFor(x => x.NuevoRolId)
            .GreaterThan(0).WithMessage("El rol indicado no es válido.");
    }
}
