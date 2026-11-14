namespace Huellitas.Application.DTOs.Adopciones;

public class AgendarVisitaDTO
{
    public DateTime Fecha { get; set; }
    public byte Modalidad { get; set; } // 0 = Presencial, 1 = Virtual
}

public class ConfirmarVisitaDTO
{
    public bool Aceptar { get; set; } // true = Confirma, false = Rechaza la fecha
}


public class VisitaResumenDTO
{
    public int VisitaId { get; set; }
    public int SolicitudId { get; set; }
    public string MascotaNombre { get; set; } = string.Empty;
    public string AdoptanteNombre { get; set; } = string.Empty;
    public string RefugioNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Modalidad { get; set; } = string.Empty; // Presencial / Virtual
    public string Estado { get; set; } = string.Empty; // Propuesta / Confirmada / etc.
}