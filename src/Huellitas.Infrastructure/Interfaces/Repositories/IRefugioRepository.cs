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
    Task<Refugios?> ObtenerPerfilPublicoPorIdAsync(int refugioId);
}