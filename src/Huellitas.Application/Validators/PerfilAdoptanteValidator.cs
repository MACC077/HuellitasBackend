using FluentValidation;
using Huellitas.Application.DTOs.Usuarios;

namespace Huellitas.Application.Validators;

public class PerfilAdoptanteValidator : AbstractValidator<ActualizarPerfilAdoptanteDTO>
{
    public PerfilAdoptanteValidator()
    {
        RuleFor(x => x.EspacioDisponible).MaximumLength(80);
        RuleFor(x => x.TiempoDisponible).MaximumLength(80);
    }
}