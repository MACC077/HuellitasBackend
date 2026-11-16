namespace Huellitas.Application.DTOs.Mascotas;

public class CrearRegistroClinicoDTO
{
    public byte Tipo { get; set; } // 0=Vacuna, 1=Desparasitacion, etc.
    public DateOnly Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? SoporteVeterinario { get; set; }
}

public class RegistroClinicoDTO
{
    public int RegistroClinicoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public DateOnly Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? SoporteVeterinario { get; set; }
}