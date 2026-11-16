using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Archivos;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Infrastructure.Interfaces.Services;

namespace Huellitas.Application.Services;

/// <summary>Valida y sube archivos a la nube.</summary>
public class ArchivoService : IArchivoService
{
    private readonly IFileStorageService _fileStorage;

    // Definimos los límites permitidos
    private readonly string[] _extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
    private const long _tamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

    public ArchivoService(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    /// <summary>Valida tamaño, extensión, tipo y contenido antes de subir el archivo.</summary>
    public async Task<ArchivoSubidoDTO> SubirArchivoAsync(Stream archivo, string nombreOriginal, string tipoArchivo)
    {
        //Validar tamaño
        if (archivo.Length > _tamanoMaximoBytes)
            throw AppException.BadRequest("El archivo supera el tamaño máximo permitido de 5 MB.");

        //Validar extensión
        var extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();
        if (!_extensionesPermitidas.Contains(extension))
            throw AppException.BadRequest($"Extensión no permitida. Use: {string.Join(", ", _extensionesPermitidas)}");

        //Validar que el tipo de archivo sea seguro (evitar carpetas arbitrarias)
        var tiposPermitidos = new[] { "perfiles", "mascotas", "soportes", "refugios", "reportes" };
        if (!tiposPermitidos.Contains(tipoArchivo.ToLowerInvariant()))
            throw AppException.BadRequest("Tipo de archivo no válido.");

        //Validar que el contenido real coincida con la extensión declarada
        Stream contenido = archivo;
        MemoryStream? copia = null;
        try
        {
            if (!archivo.CanSeek)
            {
                copia = new MemoryStream();
                await archivo.CopyToAsync(copia);
                copia.Position = 0;
                contenido = copia;
            }

            if (!await CoincideContenidoAsync(contenido, extension))
                throw AppException.BadRequest("El contenido del archivo no coincide con su extensión.");

            //Guardar en el storage (Cloudinary)
            var url = await _fileStorage.GuardarArchivoAsync(contenido, nombreOriginal, tipoArchivo.ToLowerInvariant());

            return new ArchivoSubidoDTO
            {
                Url = url,
                PublicId = Path.GetFileName(url) // O el ID real si tu storage service lo devuelve
            };
        }
        finally
        {
            if (copia != null)
                await copia.DisposeAsync();
        }
    }

    /// <summary>Comprueba la firma binaria del archivo y restaura el stream.</summary>
    private static async Task<bool> CoincideContenidoAsync(Stream contenido, string extension)
    {
        var encabezado = new byte[12];
        var leidos = 0;

        while (leidos < encabezado.Length)
        {
            var cantidad = await contenido.ReadAsync(encabezado.AsMemory(leidos, encabezado.Length - leidos));
            if (cantidad == 0)
                break;
            leidos += cantidad;
        }

        if (contenido.CanSeek)
            contenido.Position = 0;

        return extension switch
        {
            ".jpg" or ".jpeg" => leidos >= 3
                && encabezado[0] == 0xFF && encabezado[1] == 0xD8 && encabezado[2] == 0xFF,

            ".png" => leidos >= 8
                && encabezado[0] == 0x89 && encabezado[1] == 0x50
                && encabezado[2] == 0x4E && encabezado[3] == 0x47
                && encabezado[4] == 0x0D && encabezado[5] == 0x0A
                && encabezado[6] == 0x1A && encabezado[7] == 0x0A,

            ".webp" => leidos >= 12
                && encabezado[0] == (byte)'R' && encabezado[1] == (byte)'I'
                && encabezado[2] == (byte)'F' && encabezado[3] == (byte)'F'
                && encabezado[8] == (byte)'W' && encabezado[9] == (byte)'E'
                && encabezado[10] == (byte)'B' && encabezado[11] == (byte)'P',

            ".pdf" => leidos >= 4
                && encabezado[0] == (byte)'%' && encabezado[1] == (byte)'P'
                && encabezado[2] == (byte)'D' && encabezado[3] == (byte)'F',

            _ => false
        };
    }
}
