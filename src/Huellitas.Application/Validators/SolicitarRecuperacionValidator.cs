using FluentValidation;
using Huellitas.Application.DTOs.Usuarios;

namespace Huellitas.Application.Validators;

public class SolicitarRecuperacionValidator : AbstractValidator<SolicitarRecuperacionDTO>
{
    public SolicitarRecuperacionValidator()
    {
        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo es obligatorio")
            .EmailAddress().WithMessage("El formato del correo no es válido");
    }
}