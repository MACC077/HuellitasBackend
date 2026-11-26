namespace Huellitas.Infrastructure.Enums;

/// <summary>
/// Estado de moderación de una calificación. Corresponde a Calificaciones.EstadoModeracion (TINYINT).
/// La consulta pública solo devuelve las calificaciones en estado Publicada (0).
/// </summary>
public enum EstadoModeracionCalificacion : byte
{
    Publicada = 0,
    Oculta = 1
}
