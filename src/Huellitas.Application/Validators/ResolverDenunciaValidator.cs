using FluentValidation;
using Huellitas.Application.DTOs.Administracion;

namespace Huellitas.Application.Validators;

public class ResolverDenunciaValidator : AbstractValidator<ResolverDenunciaDTO>
{
    public ResolverDenunciaValidator()
    {
        RuleFor(x => x.Resultado)
            .NotEmpty().WithMessage("El resultado de la resolución es obligatorio.")
            .MaximumLength(500).WithMessage("El resultado no puede superar los 500 caracteres.");
    }
}
