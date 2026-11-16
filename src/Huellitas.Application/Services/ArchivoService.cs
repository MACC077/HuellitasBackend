using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Archivos;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Infrastructure.Interfaces.Services;

namespace Huellitas.Application.Services;

/// <summary>Valida y sube archivos a la nube.</summary>
public class ArchivoService : IArchivoService
{
    private readonly IFileStorageService _fileStorage;

    private readonly string[] _extensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
    private const long _tamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

    public ArchivoService(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    /// <summary>Valida tamaño, extensión, tipo y contenido antes de subir el archivo.</summary>
    public async Task<ArchivoSubidoDTO> SubirArchivoAsync(Stream archivo, string nombreOriginal, string tipoArchivo)
    {
        if (archivo.Length > _tamanoMaximoBytes)
            throw AppException.BadRequest("El archivo supera el tamaño máximo permitido de 5 MB.");

        var extension = Path.GetExtension(nombreOriginal).ToLowerInvariant();
        if (!_extensionesPermitidas.Contains(extension))
            throw AppException.BadRequest($"Extensión no permitida. Use: {string.Join(", ", _extensionesPermitidas)}");

        var tiposPermitidos = new[] { "perfiles", "mascotas", "soportes", "refugios", "reportes" };
        if (!tiposPermitidos.Contains(tipoArchivo.ToLowerInvariant()))
            throw AppException.BadRequest("Tipo de archivo no válido.");

        Stream contenido = archivo;
        MemoryStream? copia = null;
        try
        {
            // El stream de IFormFile es rebobinable; si no lo es, se copia para no perderlo.
            if (!archivo.CanSeek)
            {
                copia = new MemoryStream();
                await archivo.CopyToAsync(copia);
                copia.Position = 0;
                contenido = copia;
            }

            if (!await FirmasArchivo.CoincideAsync(contenido, extension))
                throw AppException.BadRequest("El contenido del archivo no coincide con su extensión.");

            var url = await _fileStorage.GuardarArchivoAsync(contenido, nombreOriginal, tipoArchivo.ToLowerInvariant());

            return new ArchivoSubidoDTO
            {
                Url = url,
                PublicId = Path.GetFileName(url)
            };
        }
        finally
        {
            if (copia != null)
                await copia.DisposeAsync();
        }
    }
}
