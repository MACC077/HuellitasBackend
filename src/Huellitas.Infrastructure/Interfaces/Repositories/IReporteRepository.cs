using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface IReporteRepository
{
    /// <summary>
    /// Inserta el reporte, asigna el número definitivo, asocia las fotografías
    /// y registra el evento inicial dentro de una única transacción.
    /// </summary>
    Task<ReportesCiudadano> InsertarAsync(
        ReportesCiudadano reporte,
        IEnumerable<ReporteMultimedia> fotos,
        CambiosDeEstado? cambioInicial);

    /// <summary>
    /// Detalle con usuario, refugio y multimedia para construir el DTO privado o público.
    /// </summary>
    Task<ReportesCiudadano?> ObtenerDetalleAsync(int reporteId);

    /// <summary>
    /// Reporte con su multimedia, usado para convertir el reporte en mascota.
    /// </summary>
    Task<ReportesCiudadano?> ObtenerParaConversionAsync(int reporteId);

    /// <summary>
    /// Datos mínimos del reporte para validar estado, propiedad y asignación.
    /// </summary>
    Task<ReportesCiudadano?> ObtenerBasicoAsync(int reporteId);

    Task<(IEnumerable<ReportesCiudadano> Items, int Total)> ObtenerBandejaAsync(
        int refugioId,
        int pagina,
        int tamano);

    Task<IEnumerable<ReportesCiudadano>> ObtenerMapaAsync();

    /// <summary>
    /// Asigna el refugio y cambia el estado de forma condicional para evitar
    /// que dos refugios tomen el mismo reporte. Registra el historial en la misma transacción.
    /// Si <paramref name="refugioId"/> es null (administrador sin refugio), el reporte
    /// queda en atención sin refugio asignado.
    /// </summary>
    Task<bool> TomarAsync(int reporteId, int? refugioId, CambiosDeEstado cambio);

    /// <summary>
    /// Cambia el estado solo si el reporte sigue en el estado anterior. Si
    /// <paramref name="refugioId"/> tiene valor, exige que el reporte pertenezca a ese
    /// refugio; si es null (administrador), permite cualquier reporte.
    /// Registra el historial en la misma transacción.
    /// </summary>
    Task<bool> CambiarEstadoAsync(
        int reporteId,
        int? refugioId,
        byte estadoAnterior,
        byte estadoNuevo,
        string? motivoCierre,
        CambiosDeEstado cambio);

    /// <summary>
    /// Vincula la mascota creada al reporte solo si sigue Rescatado y sin mascota.
    /// Debe invocarse dentro de una transacción ambiente para que la creación
    /// de la mascota y el vínculo sean atómicos.
    /// </summary>
    Task<bool> VincularMascotaAsync(
        int reporteId,
        int refugioId,
        int mascotaId,
        CambiosDeEstado cambio);

    /// <summary>
    /// Marca el reporte como inactivo (borrado lógico) y registra el evento
    /// en el historial dentro de la misma transacción. Las condiciones opcionales
    /// evitan borrar un reporte que acaba de ser tomado o convertido en mascota.
    /// </summary>
    Task<bool> EliminarAsync(
        int reporteId,
        byte? estadoEsperado,
        bool exigirSinRefugio,
        CambiosDeEstado cambio);

    /// <summary>
    /// Actualiza los datos editables del reporte activo y sin mascota vinculada.
    /// Si <paramref name="fotos"/> no es null, reemplaza las fotografías activas.
    /// Registra el historial en la misma transacción.
    /// </summary>
    Task<bool> ActualizarAsync(
        int reporteId,
        ReportesCiudadano cambios,
        IEnumerable<ReporteMultimedia>? fotos,
        CambiosDeEstado cambio);
}
