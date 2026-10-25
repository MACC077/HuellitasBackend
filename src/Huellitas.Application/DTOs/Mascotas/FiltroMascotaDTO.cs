namespace Huellitas.Application.DTOs.Mascotas;

public class FiltroMascotaDTO
{
    public int Pagina { get; set; } = 1;
    public int TamanoPagina { get; set; } = 10;
    public int? EspecieId { get; set; }
    public int? TamanoId { get; set; }
    public byte? Sexo { get; set; } // 0: Macho, 1: Hembra
}