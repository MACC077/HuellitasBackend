namespace Huellitas.Application.DTOs.Archivos;

public class ArchivoSubidoDTO
{
    public string Url { get; set; } = string.Empty;
    public string PublicId { get; set; } = string.Empty; // Por si luego quieres borrar el archivo
}