using Huellitas.Application.DTOs.Politicas;

namespace Huellitas.Application.Interfaces.Services;

public interface IPoliticaService
{
    Task<PoliticaDTO> ObtenerActivaAsync();
    Task<IEnumerable<PoliticaDTO>> ObtenerTodasAsync();
    Task<int> CrearAsync(CrearPoliticaDTO dto, int adminId);
    Task AceptarAsync(int usuarioId, AceptarPoliticaDTO dto);
}
