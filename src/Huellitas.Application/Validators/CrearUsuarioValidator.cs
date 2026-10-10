using FluentValidation;
using Huellitas.Application.DTOs.Usuarios;

namespace Huellitas.Application.Validators;

public class CrearUsuarioValidator : AbstractValidator<CrearUsuarioDTO>
{
    public CrearUsuarioValidator()
    {
        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio")
            .MaximumLength(120);

        RuleFor(x => x.Correo)
            .NotEmpty().WithMessage("El correo es obligatorio")
            .EmailAddress().WithMessage("El formato del correo no es válido")
            .MaximumLength(150);

        RuleFor(x => x.Contrasena)
            .NotEmpty().WithMessage("La contraseña es obligatoria")
            .MinimumLength(8).WithMessage("La contraseña debe tener mínimo 8 caracteres")
            .MaximumLength(100);

        RuleFor(x => x.Telefono)
            .NotEmpty().WithMessage("El teléfono es obligatorio")
            .Matches(@"^\d{10}$")
            .WithMessage("El teléfono debe tener exactamente 10 dígitos");

        RuleFor(x => x.AceptaPolitica)
            .Equal(true)
            .WithMessage("Debe aceptar la política de tratamiento de datos (Ley 1581 de 2012)");
    }
}