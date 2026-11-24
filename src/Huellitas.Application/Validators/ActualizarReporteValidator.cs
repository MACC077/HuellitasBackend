using FluentValidation;
using Huellitas.Application.DTOs.Reportes;
using Huellitas.Infrastructure.Enums;

namespace Huellitas.Application.Validators;

/// <summary>
/// Mismas reglas de campos que <see cref="CrearReporteValidator"/>, aplicadas a la edición.
/// </summary>
public class ActualizarReporteValidator : AbstractValidator<ActualizarReporteDTO>
{
    public ActualizarReporteValidator()
    {
        RuleFor(x => x.Tipo)
            .Must(SerTipoValido).WithMessage("El tipo de reporte no es válido.");

        RuleFor(x => x.Descripcion)
            .NotEmpty().WithMessage("La descripción del reporte es obligatoria.")
            .MaximumLength(1000).WithMessage("La descripción no puede superar los 1000 caracteres.");

        RuleFor(x => x.Latitud)
            .InclusiveBetween(-90m, 90m).WithMessage("La latitud debe estar entre -90 y 90.");

        RuleFor(x => x.Longitud)
            .InclusiveBetween(-180m, 180m).WithMessage("La longitud debe estar entre -180 y 180.");

        RuleFor(x => x.Direccion)
            .MaximumLength(200).WithMessage("La dirección no puede superar los 200 caracteres.");

        RuleFor(x => x.PuntoReferencia)
            .MaximumLength(200).WithMessage("El punto de referencia no puede superar los 200 caracteres.");

        RuleForEach(x => x.FotosUrls)
            .NotEmpty().WithMessage("Las URLs de las fotografías no pueden estar vacías.")
            .MaximumLength(400).WithMessage("La URL de una fotografía supera la longitud permitida.")
            .Must(SerUrlValida).WithMessage("Una de las fotografías no tiene una URL válida (http o https).");
    }

    private static bool SerTipoValido(byte valor)
    {
        return Enum.IsDefined(typeof(TipoReporte), valor);
    }

    /// <summary>
    /// Acepta cualquier URL absoluta http/https, incluidas externas (Drive, etc.).
    /// </summary>
    private static bool SerUrlValida(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
