using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface IAdminRepository
{
    // ADM-01: Usuarios
    Task<(IEnumerable<Usuarios> Items, int Total)> ObtenerUsuariosAsync(
        int pagina,
        int tamano,
        string? busqueda,
        int? rolId = null,
        byte? estado = null);

    Task<Usuarios?> ObtenerUsuarioConRolAsync(int usuarioId);
    Task<Roles?> ObtenerRolAsync(int rolId);
    Task<int> ContarAdministradoresActivosAsync(int excluyendoUsuarioId);
    Task ActualizarUsuarioAsync(Usuarios usuario);

    // ADM-02: Denuncias
    Task<(IEnumerable<Denuncias> Items, int Total)> ObtenerDenunciasAsync(
        int pagina,
        int tamano,
        byte? estado);

    Task<Denuncias?> ObtenerDenunciaAsync(int denunciaId);

    /// <summary>
    /// Marca la denuncia como resuelta solo si aún no lo estaba. Devuelve false si
    /// otra solicitud concurrente ya la resolvió.
    /// </summary>
    Task<bool> MarcarDenunciaResueltaAsync(int denunciaId, string resultado, DateTime fecha);
    Task<bool> ExisteContenidoAsync(byte tipoContenido, int idContenido);

    /// <summary>
    /// Aplica el ocultamiento propio del tipo de contenido. Devuelve true si el
    /// registro estaba visible y fue ocultado; false si ya estaba oculto.
    /// </summary>
    Task<bool> OcultarContenidoAsync(byte tipoContenido, int idContenido, DateTime fecha);

    // ADM-03: Configuración
    Task<IEnumerable<ConfiguracionSistema>> ObtenerConfiguracionesAsync();
    Task<ConfiguracionSistema?> ObtenerConfiguracionAsync(int configuracionId);
    Task<bool> ExisteClaveConfiguracionAsync(string clave, int? excluyendoId);
    Task InsertarConfiguracionAsync(ConfiguracionSistema configuracion);
    Task ActualizarConfiguracionAsync(ConfiguracionSistema configuracion);

    // ADM-04: Dashboard
    Task<int> ContarAdopcionesExitosasAsync();
    Task<int> ContarMascotasDisponiblesAsync();
    Task<int> ContarUsuariosActivosAsync();
    Task<int> ContarRefugiosVerificadosAsync();
    Task<int> ContarReportesCiudadanosAsync();
    Task<double> ObtenerTiempoPromedioAdopcionDiasAsync();

    /// <summary>
    /// Totales de reportes ciudadanos agrupados por estado. Si se indica un estado,
    /// solo devuelve ese grupo.
    /// </summary>
    Task<IEnumerable<(byte Estado, int Total)>> ObtenerReportesPorEstadoAsync(byte? estado = null);
}
