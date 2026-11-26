namespace Huellitas.Application.DTOs.Reportes;

public class ConvertirMascotaDTO
{
    public string Nombre { get; set; } = string.Empty;
    public int EspecieId { get; set; }
    public int? RazaId { get; set; }
    public int TamanoId { get; set; }
    public int ColorId { get; set; }
    public byte Sexo { get; set; }
    public byte? EdadAproximada { get; set; }
    public string? Descripcion { get; set; }
    public bool Esterilizada { get; set; }
}
