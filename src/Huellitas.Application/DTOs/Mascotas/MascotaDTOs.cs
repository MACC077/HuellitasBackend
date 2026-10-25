namespace Huellitas.Application.DTOs.Mascotas;

public class CrearMascotaDTO
{
    public string Nombre { get; set; } = string.Empty;
    public int EspecieId { get; set; }
    public int? RazaId { get; set; }
    public int TamanoId { get; set; }
    public int ColorId { get; set; }
    public byte Sexo { get; set; } // 0 = Macho, 1 = Hembra
    public byte? EdadAproximada { get; set; }
    public string? Descripcion { get; set; }
    public bool Esterilizada { get; set; }
    public byte Procedencia { get; set; } // 0=Rescate, 1=Entrega, 2=Reporte
    public List<string> FotosUrls { get; set; } = new();
}

public class ActualizarMascotaDTO
{
    public string Nombre { get; set; } = string.Empty;
    public int? RazaId { get; set; }
    public int TamanoId { get; set; }
    public int ColorId { get; set; }
    public byte Sexo { get; set; }
    public byte? EdadAproximada { get; set; }
    public string? Descripcion { get; set; }
    public bool Esterilizada { get; set; }
    public List<string> FotosUrls { get; set; } = new(); // Opcional, si se envían, se reemplazan
}

public class CambiarEstadoMascotaDTO
{
    public byte NuevoEstado { get; set; } // 0=Disponible, 1=EnProceso, 2=Adoptada, 3=NoDisponible
    public string? Motivo { get; set; }
}

public class MascotaResumenDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public string? Raza { get; set; }
    public string Sexo { get; set; } = string.Empty;
    public int EdadAproximada { get; set; }
    public string? FotoPrincipal { get; set; }
    public string Estado { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}