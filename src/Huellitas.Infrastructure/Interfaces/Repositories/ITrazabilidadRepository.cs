using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface ITrazabilidadRepository
{
    Task InsertarCambioEstadoAsync(CambiosDeEstado cambio);
    Task<IEnumerable<CambiosDeEstado>> ObtenerCambiosEstadoAsync(string entidad, int idEntidad);
    Task InsertarBitacoraAsync(Bitacoras bitacora);
    Task<IEnumerable<Bitacoras>> ObtenerBitacorasAsync(int mascotaId);
    Task InsertarAuditoriaAsync(RegistrosAuditorias auditoria);
    Task<(IEnumerable<RegistrosAuditorias> Items, int Total)> ObtenerAuditoriaAsync(int pagina, int tamano);
}
