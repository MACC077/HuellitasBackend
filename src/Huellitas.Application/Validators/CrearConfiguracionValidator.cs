using FluentValidation;
using Huellitas.Application.DTOs.Administracion;

namespace Huellitas.Application.Validators;

public class CrearConfiguracionValidator : AbstractValidator<CrearConfiguracionDTO>
{
    public CrearConfiguracionValidator()
    {
        RuleFor(x => x.Clave)
            .NotEmpty().WithMessage("La clave de configuración es obligatoria.")
            .MaximumLength(80).WithMessage("La clave no puede superar los 80 caracteres.")
            .Matches(@"^[A-Za-z0-9_.:-]+$")
            .WithMessage("La clave solo puede contener letras, números, punto, guion, guion bajo y dos puntos.");

        RuleFor(x => x.Valor)
            .NotEmpty().WithMessage("El valor de configuración es obligatorio.")
            .MaximumLength(400).WithMessage("El valor no puede superar los 400 caracteres.");

        RuleFor(x => x.Descripcion)
            .MaximumLength(300).WithMessage("La descripción no puede superar los 300 caracteres.");
    }
}
