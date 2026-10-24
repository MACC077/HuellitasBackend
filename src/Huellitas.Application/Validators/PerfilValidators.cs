using FluentValidation;
using Huellitas.Application.DTOs.Usuarios;

namespace Huellitas.Application.Validators;

public class ActualizarPerfilValidator : AbstractValidator<ActualizarPerfilDTO>
{
    public ActualizarPerfilValidator()
    {
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Telefono).Matches(@"^\d{10}$").When(x => !string.IsNullOrEmpty(x.Telefono));
    }
}

public class CambiarPasswordValidator : AbstractValidator<CambiarPasswordDTO>
{
    public CambiarPasswordValidator()
    {
        RuleFor(x => x.PasswordActual).NotEmpty();
        RuleFor(x => x.PasswordNueva).NotEmpty().MinimumLength(8);
    }
}

public class CambiarCorreoValidator : AbstractValidator<CambiarCorreoDTO>
{
    public CambiarCorreoValidator()
    {
        RuleFor(x => x.NuevoCorreo).NotEmpty().EmailAddress();
        RuleFor(x => x.PasswordActual).NotEmpty();
    }
}