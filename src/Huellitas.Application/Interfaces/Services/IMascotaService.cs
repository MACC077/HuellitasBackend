using Huellitas.Application.DTOs.Mascotas;
using Huellitas.Application.Common; // Asumiendo que aquí tienes tu PagedResult

namespace Huellitas.Application.Interfaces.Services;

public interface IMascotaService
{
    Task<int> RegistrarAsync(CrearMascotaDTO dto, int usuarioId);
    Task ActualizarAsync(int mascotaId, ActualizarMascotaDTO dto, int usuarioId);
    Task CambiarEstadoAsync(int mascotaId, CambiarEstadoMascotaDTO dto, int usuarioId);
    Task DarDeBajaAsync(int mascotaId, string motivo, int usuarioId);
    Task<PagedResult<MascotaResumenDTO>> ObtenerMisMascotasAsync(int usuarioId, int pagina, int tamano);
    Task<PagedResult<MascotaResumenDTO>> ObtenerCatalogoAsync(FiltroMascotaDTO filtro);
    Task<MascotaDetalleDTO> ObtenerDetalleAsync(int mascotaId);
    Task AgregarRegistroClinicoAsync(int mascotaId, CrearRegistroClinicoDTO dto, int usuarioId);
    Task<PagedResult<RegistroClinicoDTO>> ObtenerHistorialClinicoAsync(int mascotaId, int pagina, int tamano);
}