namespace Huellitas.Application.DTOs.Mascotas;

public class RecomendacionMascotaDTO : MascotaResumenDTO
{
    public int Puntaje { get; set; } // 0 a 100
    public List<string> Factores { get; set; } = new(); // Ej: "Ideal para apartamento", "Bueno con niños"
}