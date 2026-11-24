namespace Huellitas.Infrastructure.Enums;

/// <summary>
/// Mapea los valores de la columna TINYINT Estado de ReportesCiudadanos.
/// </summary>
public enum EstadoReporte : byte
{
    Recibido = 0,
    EnAtencion = 1,
    Rescatado = 2,
    NoEncontrado = 3,
    Cerrado = 4
}
