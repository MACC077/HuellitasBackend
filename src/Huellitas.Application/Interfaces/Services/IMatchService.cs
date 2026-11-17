using Huellitas.Application.DTOs.Mascotas;

namespace Huellitas.Application.Interfaces.Services;

public interface IMatchService
{
    Task<IEnumerable<RecomendacionMascotaDTO>> ObtenerRecomendacionesAsync(int usuarioId);
}