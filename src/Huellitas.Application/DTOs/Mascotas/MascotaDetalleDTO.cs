namespace Huellitas.Application.DTOs.Mascotas;

public class MascotaDetalleDTO
{
    public int Id { get; set; }
    public int RefugioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public int EspecieId { get; set; }
    public int? RazaId { get; set; }
    public int TamanoId { get; set; }
    public int ColorId { get; set; }
    public string? Raza { get; set; }
    public string Sexo { get; set; } = string.Empty;
    public int EdadAproximada { get; set; }
    public string? Descripcion { get; set; }
    public bool Esterilizada { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Refugio { get; set; } = string.Empty;
    public string? RefugioTelefono { get; set; }
    public List<string> Fotos { get; set; } = new();
}