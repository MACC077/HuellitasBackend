using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Huellitas.Infrastructure.Interfaces.Services;

namespace Huellitas.Infrastructure.Services;

public class CloudinaryFileStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryFileStorageService(Cloudinary cloudinary) => _cloudinary = cloudinary;

    public async Task<string> GuardarArchivoAsync(Stream archivo, string nombreOriginal, string carpetaDestino)
    {
        var extension = Path.GetExtension(nombreOriginal).ToLower();
        var esImagen = extension is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp";

        //AQUÍ SE CONFIGURA LA RUTA EN CLOUDINARY
        // Si carpetaDestino = "mascotas", la ruta será "huellitas/mascotas"
        var folderPath = $"huellitas/{carpetaDestino}";
        var publicId = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(nombreOriginal)}";

        if (esImagen)
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(nombreOriginal, archivo),
                Folder = folderPath,
                PublicId = publicId,
                Overwrite = true
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl?.ToString() ?? throw new Exception("Error al subir imagen.");
        }
        else
        {
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(nombreOriginal, archivo),
                Folder = folderPath,
                PublicId = publicId,
                Overwrite = true
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return result.SecureUrl?.ToString() ?? throw new Exception("Error al subir documento.");
        }
    }
}