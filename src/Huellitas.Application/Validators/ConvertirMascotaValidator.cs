using FluentValidation;
using Huellitas.Application.DTOs.Reportes;

namespace Huellitas.Application.Validators;

public class ConvertirMascotaValidator : AbstractValidator<ConvertirMascotaDTO>
{
    public ConvertirMascotaValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre de la mascota es obligatorio.")
            .MaximumLength(80).WithMessage("El nombre de la mascota no puede superar los 80 caracteres.");

        RuleFor(x => x.EspecieId)
            .GreaterThan(0).WithMessage("La especie es obligatoria.");

        RuleFor(x => x.RazaId)
            .GreaterThan(0).WithMessage("La raza indicada no es válida.")
            .When(x => x.RazaId.HasValue);

        RuleFor(x => x.TamanoId)
            .GreaterThan(0).WithMessage("El tamaño es obligatorio.");

        RuleFor(x => x.ColorId)
            .GreaterThan(0).WithMessage("El color es obligatorio.");

        RuleFor(x => x.Sexo)
            .LessThanOrEqualTo((byte)1).WithMessage("El sexo debe ser 0 (Macho) o 1 (Hembra).");

        RuleFor(x => x.Descripcion)
            .MaximumLength(1000).WithMessage("La descripción no puede superar los 1000 caracteres.");
    }
}
