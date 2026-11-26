namespace Huellitas.Application.DTOs.Adopciones;

public class DocumentoSoporteDTO
{
    public int DocumentoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string ArchivoUrl { get; set; } = string.Empty;
    public DateTime FechaCarga { get; set; }
}