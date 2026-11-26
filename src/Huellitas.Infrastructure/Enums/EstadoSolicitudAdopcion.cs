namespace Huellitas.Infrastructure.Enums;

public enum EstadoSolicitudAdopcion : byte
{
    Enviada = 0,
    EnRevision = 1,
    Entrevista = 2,
    Aprobada = 3,
    Rechazada = 4,
    Finalizada = 5,
    Cancelada = 6,
    Cerrada = 7 // Cuando se cierra automáticamente porque otro adoptó
}