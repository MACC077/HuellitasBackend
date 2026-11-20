namespace Huellitas.Application.DTOs.Trazabilidad;

public class LineaTiempoDTO
{
    public string EstadoAnterior { get; set; } = string.Empty;
    public string EstadoNuevo { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public string UsuarioResponsable { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}

public class HistorialMascotaDTO
{
    public string TipoEvento { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}

public class NotificacionDTO
{
    public long NotificacionId { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public bool Leida { get; set; }
    public DateTime FechaEnvio { get; set; }
}

public class AuditoriaDTO
{
    public long AuditoriaId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string EntidadAfectada { get; set; } = string.Empty;
    public int? IdEntidad { get; set; }
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public string UsuarioResponsable { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}
