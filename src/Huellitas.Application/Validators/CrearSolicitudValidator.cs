using FluentValidation;
using Huellitas.Application.DTOs.Adopciones;

namespace Huellitas.Application.Validators;

public class CrearSolicitudValidator : AbstractValidator<CrearSolicitudDTO>
{
    public CrearSolicitudValidator()
    {
        RuleFor(x => x.MascotaId)
            .GreaterThan(0).WithMessage("El ID de la mascota es inválido.");

        RuleFor(x => x.RespuestasFormulario)
            .NotNull().WithMessage("Las respuestas del formulario son obligatorias.")
            .Must(NoSerStringVacio).WithMessage("La motivación no puede estar vacía.");
    }

    private bool NoSerStringVacio(object respuestas)
    {
        // Si es un string, validamos que no esté vacío
        if (respuestas is string texto)
            return !string.IsNullOrWhiteSpace(texto);

        // Si es un objeto, con que no sea null es suficiente
        return respuestas != null;
    }
}