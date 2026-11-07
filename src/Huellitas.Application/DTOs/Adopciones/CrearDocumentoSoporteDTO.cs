public class CrearDocumentoSoporteDTO
{
    public string Tipo { get; set; } = string.Empty; // "Cédula", "Certificado"
    public string ArchivoUrl { get; set; } = string.Empty; // Recibe la URL de Cloudinary
}