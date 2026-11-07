using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Archivos;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Infrastructure.Interfaces.Services;

namespace Huellitas.Application.Services;

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

    public async Task<ArchivoSubidoDTO> SubirArchivoAsync(Stream archivo, string nombreOriginal, string tipoArchivo)
    {
        //Validar tamaño
        if (archivo.Length > _tamanoMaximoBytes)
            throw AppException.BadRequest("El archivo supera el tamaño máximo permitido de 5 MB.");

        //Validar extensión
        var extension = Path.GetExtension(nombreOriginal).ToLower();
        if (!_extensionesPermitidas.Contains(extension))
            throw AppException.BadRequest($"Extensión no permitida. Use: {string.Join(", ", _extensionesPermitidas)}");

        //Validar que el tipo de archivo sea seguro (evitar carpetas arbitrarias)
        var tiposPermitidos = new[] { "perfiles", "mascotas", "soportes", "refugios", "reportes" };
        if (!tiposPermitidos.Contains(tipoArchivo.ToLower()))
            throw AppException.BadRequest("Tipo de archivo no válido.");

        //Guardar en el storage (Cloudinary)
        var url = await _fileStorage.GuardarArchivoAsync(archivo, nombreOriginal, tipoArchivo.ToLower());

        return new ArchivoSubidoDTO
        {
            Url = url,
            PublicId = Path.GetFileName(url) // O el ID real si tu storage service lo devuelve
        };
    }
}