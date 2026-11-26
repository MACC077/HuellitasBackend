using Huellitas.Application.DTOs.Denuncias;

namespace Huellitas.Application.Interfaces.Services;

public interface IDenunciaService
{
    Task<int> CrearAsync(int usuarioId, CrearDenunciaDTO dto);
}
