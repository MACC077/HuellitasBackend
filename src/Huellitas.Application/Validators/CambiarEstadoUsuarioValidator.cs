using FluentValidation;
using Huellitas.Application.DTOs.Administracion;

namespace Huellitas.Application.Validators;

public class CambiarEstadoUsuarioValidator : AbstractValidator<CambiarEstadoUsuarioDTO>
{
    public CambiarEstadoUsuarioValidator()
    {
        RuleFor(x => x.Motivo)
            .MaximumLength(500).WithMessage("El motivo no puede superar los 500 caracteres.");

        When(x => !x.Activo, () =>
        {
            RuleFor(x => x.Motivo)
                .NotEmpty().WithMessage("El motivo es obligatorio al desactivar un usuario.");
        });
    }
}
