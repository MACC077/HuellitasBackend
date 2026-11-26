using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface IRefugioRepository
{
    Task<Refugios?> ObtenerPorUsuarioIdAsync(int usuarioId);
    Task<Refugios?> ObtenerPorIdAsync(int refugioId);
    Task<Refugios> InsertarAsync(Refugios refugio);
    Task ActualizarAsync(Refugios refugio);
    Task<IEnumerable<RequisitosAdopcion>> ObtenerRequisitosPorRefugioAsync(int refugioId);
    Task InsertarRequisitoAsync(RequisitosAdopcion requisito);
    Task EliminarRequisitoAsync(int requisitoId);
    Task<(IEnumerable<Refugios> Items, int Total)> ObtenerRefugiosPublicosAsync(int pagina, int tamano);

    /// <summary>
    /// Listado administrativo de refugios por estado de verificación.
    /// </summary>
    Task<(IEnumerable<Refugios> Items, int Total)> ObtenerRefugiosPorEstadoAsync(
        byte estado,
        int pagina,
        int tamano);

    Task<Refugios?> ObtenerPerfilPublicoPorIdAsync(int refugioId);
    Task<bool> ExisteNitAsync(string nit);

    Task InsertarSoportesAsync(IEnumerable<RefugioSoportes> soportes);
    Task<IEnumerable<RefugioSoportes>> ObtenerSoportesAsync(int refugioId);
}