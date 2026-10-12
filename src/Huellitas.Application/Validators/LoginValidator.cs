using FluentValidation;
using Huellitas.Application.DTOs.Auth;

namespace Huellitas.Application.Validators;

public class LoginValidator : AbstractValidator<LoginDTO>
{
    public LoginValidator()
    {
        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo es obligatorio")
            .EmailAddress().WithMessage("El formato del correo no es válido");

        RuleFor(x => x.Contrasena)
            .NotEmpty().WithMessage("La contraseña es obligatoria");
    }
}