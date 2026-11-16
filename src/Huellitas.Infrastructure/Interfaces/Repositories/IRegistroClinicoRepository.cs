using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface IRegistroClinicoRepository
{
    Task<(IEnumerable<RegistrosClinico> Items, int Total)> ObtenerPorMascotaAsync(int mascotaId, int pagina, int tamano);
    Task InsertarAsync(RegistrosClinico registro);
}