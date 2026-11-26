using Huellitas.Application.Common;
using Huellitas.Application.DTOs.Administracion;

namespace Huellitas.Application.Interfaces.Services;

public interface IAdminService
{
    // ADM-01
    Task<PagedResult<UsuarioAdminDTO>> ObtenerUsuariosAsync(int pagina, int tamano, string? busqueda);

    /// <summary>
    /// Devuelve true si el estado cambió; false si el usuario ya tenía ese estado.
    /// </summary>
    Task<bool> CambiarEstadoUsuarioAsync(int usuarioId, CambiarEstadoUsuarioDTO dto, int adminId);

    /// <summary>
    /// Devuelve true si el rol cambió; false si el usuario ya tenía ese rol.
    /// </summary>
    Task<bool> CambiarRolUsuarioAsync(int usuarioId, CambiarRolUsuarioDTO dto, int adminId);

    // ADM-02
    Task<PagedResult<DenunciaDTO>> ObtenerDenunciasAsync(int pagina, int tamano, byte? estado);
    Task ResolverDenunciaAsync(int denunciaId, ResolverDenunciaDTO dto, int adminId);

    // ADM-03
    Task<IEnumerable<ConfiguracionDTO>> ObtenerConfiguracionesAsync();
    Task<int> CrearConfiguracionAsync(CrearConfiguracionDTO dto, int adminId);
    Task ActualizarConfiguracionAsync(int configuracionId, CrearConfiguracionDTO dto, int adminId);

    // ADM-04
    Task<DashboardMetricasDTO> ObtenerMetricasAsync();
}
