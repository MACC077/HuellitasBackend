using FluentValidation;
using Huellitas.Application.DTOs.Adopciones;

namespace Huellitas.Application.Validators;

public class RechazarSolicitudValidator : AbstractValidator<RechazarSolicitudDTO>
{
    public RechazarSolicitudValidator()
    {
        RuleFor(x => x.MotivoRechazo)
            .NotEmpty().WithMessage("El motivo de rechazo es obligatorio.")
            .MaximumLength(500);
    }
}