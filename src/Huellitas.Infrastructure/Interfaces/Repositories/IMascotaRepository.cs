using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface IMascotaRepository
{
    Task<Mascotas> InsertarAsync(Mascotas mascota);
    Task ActualizarAsync(Mascotas mascota);
    Task<Mascotas?> ObtenerPorIdAsync(int mascotaId);
    Task<bool> TieneSolicitudesActivasAsync(int mascotaId);
    Task<(IEnumerable<Mascotas> Items, int Total)> ObtenerMisMascotasAsync(int refugioId, int pagina, int tamano);

    // Métodos para el historial y fotos
    Task InsertarBitacoraAsync(Bitacoras bitacora);
    Task InsertarMultimediaAsync(Multimedia multimedia);
    Task EliminarMultimediaAsync(int mascotaId); // Para actualizar fotos
}