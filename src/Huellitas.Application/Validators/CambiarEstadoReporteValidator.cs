using FluentValidation;
using Huellitas.Application.DTOs.Reportes;
using Huellitas.Infrastructure.Enums;

namespace Huellitas.Application.Validators;

public class CambiarEstadoReporteValidator : AbstractValidator<CambiarEstadoReporteDTO>
{
    public CambiarEstadoReporteValidator()
    {
        RuleFor(x => x.NuevoEstado)
            .Must(SerEstadoValido).WithMessage("El estado solicitado no es válido.");

        RuleFor(x => x.Motivo)
            .MaximumLength(500).WithMessage("El motivo no puede superar los 500 caracteres.");

        When(x => x.NuevoEstado == (byte)EstadoReporte.NoEncontrado ||
                  x.NuevoEstado == (byte)EstadoReporte.Cerrado, () =>
        {
            RuleFor(x => x.Motivo)
                .NotEmpty().WithMessage("El motivo es obligatorio para el estado seleccionado.");
        });
    }

    private static bool SerEstadoValido(byte valor)
    {
        return Enum.IsDefined(typeof(EstadoReporte), valor);
    }
}
