using Huellitas.Application.Common.Exceptions;

namespace Huellitas.Application.Common;

/// <summary>
/// Validación compartida de soportes de refugio (PDF o imagen) recibidos como URLs.
/// </summary>
public static class SoportesValidator
{
    private static readonly string[] ExtensionesPermitidas = { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };

    public static IReadOnlyList<string> Validar(IEnumerable<string>? urls)
    {
        if (urls is null)
            return Array.Empty<string>();

        var resultado = new List<string>();

        foreach (var url in urls)
        {
            var valor = url?.Trim();

            if (string.IsNullOrWhiteSpace(valor))
                continue;

            if (!Uri.TryCreate(valor, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw AppException.BadRequest("Los soportes deben ser URLs válidas (http o https).");
            }

            var extension = Path.GetExtension(uri.AbsolutePath).ToLowerInvariant();

            if (!ExtensionesPermitidas.Contains(extension))
                throw AppException.BadRequest("Los soportes deben ser PDF o imágenes (jpg, jpeg, png, webp).");

            resultado.Add(valor);
        }

        return resultado;
    }

    public static string Tipo(string url)
    {
        var extension = Path.GetExtension(new Uri(url).AbsolutePath);
        return extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase) ? "PDF" : "Imagen";
    }
}
