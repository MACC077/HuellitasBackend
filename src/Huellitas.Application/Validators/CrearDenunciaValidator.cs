using FluentValidation;
using Huellitas.Application.DTOs.Denuncias;
using Huellitas.Infrastructure.Enums;

namespace Huellitas.Application.Validators;

public class CrearDenunciaValidator : AbstractValidator<CrearDenunciaDTO>
{
    public CrearDenunciaValidator()
    {
        RuleFor(x => x.TipoContenido)
            .Must(v => Enum.IsDefined(typeof(TipoContenidoDenuncia), v))
            .WithMessage("El tipo de contenido denunciado no es válido.");

        RuleFor(x => x.IdContenido)
            .GreaterThan(0).WithMessage("El identificador del contenido denunciado es obligatorio.");

        RuleFor(x => x.Motivo)
            .Must(v => Enum.IsDefined(typeof(MotivoDenuncia), v))
            .WithMessage("El motivo de la denuncia no es válido.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000).WithMessage("La descripción no puede superar los 1000 caracteres.");
    }
}
