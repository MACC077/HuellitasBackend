namespace Huellitas.Infrastructure.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> GuardarArchivoAsync(Stream archivo, string nombreOriginal, string carpetaDestino);
}