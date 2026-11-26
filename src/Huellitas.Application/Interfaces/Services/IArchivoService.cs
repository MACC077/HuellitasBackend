using Huellitas.Application.DTOs.Archivos;

namespace Huellitas.Application.Interfaces.Services;

public interface IArchivoService
{
    Task<ArchivoSubidoDTO> SubirArchivoAsync(Stream archivo, string nombreOriginal, string tipoArchivo);
}