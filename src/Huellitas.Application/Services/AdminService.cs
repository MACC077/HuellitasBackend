using System.Transactions;
using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Administracion;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Mappings;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Services;

public class AdminService : IAdminService
{
    private static readonly string[] FragmentosClaveSensible =
    {
        "secret", "password", "contrasena", "contraseña", "token",
        "apikey", "api_key", "credential", "credencial",
        "privatekey", "private_key", "connectionstring", "connection_string"
    };

    private readonly IAdminRepository _repo;
    private readonly ITrazabilidadRepository _trazabilidad;

    public AdminService(IAdminRepository repo, ITrazabilidadRepository trazabilidad)
    {
        _repo = repo;
        _trazabilidad = trazabilidad;
    }

    // ADM-01: Usuarios

    public async Task<PagedResult<UsuarioAdminDTO>> ObtenerUsuariosAsync(
        int pagina,
        int tamano,
        string? busqueda)
    {
        var (paginaValida, tamanoValido) = Paginacion.Validar(pagina, tamano);
        var termino = string.IsNullOrWhiteSpace(busqueda) ? null : busqueda.Trim();

        var (items, total) = await _repo.ObtenerUsuariosAsync(paginaValida, tamanoValido, termino);

        return new PagedResult<UsuarioAdminDTO>
        {
            Items = items.Select(u => u.ToAdminDto()).ToList(),
            Pagina = paginaValida,
            TamanoPagina = tamanoValido,
            TotalRegistros = total
        };
    }

    public async Task<bool> CambiarEstadoUsuarioAsync(
        int usuarioId,
        CambiarEstadoUsuarioDTO dto,
        int adminId)
    {
        var usuario = await _repo.ObtenerUsuarioConRolAsync(usuarioId)
            ?? throw AppException.NotFound("El usuario no existe.");

        var motivo = string.IsNullOrWhiteSpace(dto.Motivo) ? null : dto.Motivo.Trim();

        if (!dto.Activo && motivo is null)
            throw AppException.BadRequest("El motivo es obligatorio al desactivar un usuario.");

        if (!dto.Activo && usuarioId == adminId)
            throw AppException.BadRequest("No puedes desactivar tu propia cuenta.");

        var estadoNuevo = dto.Activo ? EstadoUsuario.Activo : EstadoUsuario.Desactivado;

        if (usuario.Activo == dto.Activo && usuario.Estado == (byte)estadoNuevo)
            return false;

        var eraAdminActivo = usuario.RolId == (int)RolUsuario.Administrador &&
                             usuario.Activo &&
                             usuario.Estado == (byte)EstadoUsuario.Activo;

        if (!dto.Activo && eraAdminActivo)
        {
            var otrosAdministradores = await _repo.ContarAdministradoresActivosAsync(usuarioId);

            if (otrosAdministradores == 0)
                throw AppException.Conflict("No se puede desactivar al último administrador activo.");
        }

        var ahora = DateTime.Now;
        var activoAnterior = usuario.Activo;
        var estadoAnterior = usuario.Estado;

        using var scope = CrearTransaccion();

        usuario.Activo = dto.Activo;
        usuario.Estado = (byte)estadoNuevo;
        usuario.FechaModificacion = ahora;

        if (dto.Activo)
        {
            usuario.IntentosFallidos = 0;
            usuario.FechaBloqueo = null;
        }

        await _repo.ActualizarUsuarioAsync(usuario);

        await _trazabilidad.InsertarAuditoriaAsync(CrearAuditoria(
            adminId,
            "CambiarEstadoUsuario",
            "Usuarios",
            usuarioId,
            $"Activo={activoAnterior}; Estado={NombreEstadoUsuario(estadoAnterior)}",
            $"Activo={dto.Activo}; Estado={estadoNuevo}; Motivo={motivo ?? "N/A"}",
            ahora));

        scope.Complete();
        return true;
    }

    public async Task<bool> CambiarRolUsuarioAsync(
        int usuarioId,
        CambiarRolUsuarioDTO dto,
        int adminId)
    {
        var usuario = await _repo.ObtenerUsuarioConRolAsync(usuarioId)
            ?? throw AppException.NotFound("El usuario no existe.");

        var rol = await _repo.ObtenerRolAsync(dto.NuevoRolId)
            ?? throw AppException.BadRequest("El rol indicado no existe.");

        if (!rol.Activo)
            throw AppException.BadRequest("El rol indicado no está activo.");

        if (usuario.RolId == rol.RolId)
            return false;

        var eraAdminActivo = usuario.RolId == (int)RolUsuario.Administrador &&
                             usuario.Activo &&
                             usuario.Estado == (byte)EstadoUsuario.Activo;

        if (eraAdminActivo && rol.RolId != (int)RolUsuario.Administrador)
        {
            var otrosAdministradores = await _repo.ContarAdministradoresActivosAsync(usuarioId);

            if (otrosAdministradores == 0)
                throw AppException.Conflict("No se puede retirar el rol al último administrador activo.");
        }

        var ahora = DateTime.Now;
        var rolAnterior = usuario.Rol?.Nombre ?? usuario.RolId.ToString();
        var refugioAnterior = usuario.RefugioId;

        using var scope = CrearTransaccion();

        usuario.RolId = rol.RolId;

        // Un usuario que deja de ser Refugio no debe conservar el vínculo con un refugio.
        if (rol.RolId != (int)RolUsuario.Refugio)
            usuario.RefugioId = null;

        usuario.FechaModificacion = ahora;

        await _repo.ActualizarUsuarioAsync(usuario);

        var valorNuevo = $"Rol={rol.Nombre}";
        if (rol.RolId != (int)RolUsuario.Refugio && refugioAnterior.HasValue)
            valorNuevo += $"; RefugioId eliminado={refugioAnterior.Value}";

        await _trazabilidad.InsertarAuditoriaAsync(CrearAuditoria(
            adminId,
            "CambiarRolUsuario",
            "Usuarios",
            usuarioId,
            $"Rol={rolAnterior}",
            valorNuevo,
            ahora));

        scope.Complete();
        return true;
    }

    // ADM-02: Denuncias

    public async Task<PagedResult<DenunciaDTO>> ObtenerDenunciasAsync(
        int pagina,
        int tamano,
        byte? estado)
    {
        var (paginaValida, tamanoValido) = Paginacion.Validar(pagina, tamano);

        if (estado.HasValue && !Enum.IsDefined(typeof(EstadoDenuncia), estado.Value))
            throw AppException.BadRequest("El estado de denuncia no es válido.");

        var (items, total) = await _repo.ObtenerDenunciasAsync(paginaValida, tamanoValido, estado);

        return new PagedResult<DenunciaDTO>
        {
            Items = items.Select(d => d.ToDto()).ToList(),
            Pagina = paginaValida,
            TamanoPagina = tamanoValido,
            TotalRegistros = total
        };
    }

    public async Task ResolverDenunciaAsync(
        int denunciaId,
        ResolverDenunciaDTO dto,
        int adminId)
    {
        var denuncia = await _repo.ObtenerDenunciaAsync(denunciaId)
            ?? throw AppException.NotFound("La denuncia no existe.");

        if (denuncia.Estado == (byte)EstadoDenuncia.Resuelta)
            throw AppException.Conflict("La denuncia ya fue resuelta.");

        if (denuncia.Estado != (byte)EstadoDenuncia.Recibida &&
            denuncia.Estado != (byte)EstadoDenuncia.EnRevision)
        {
            throw AppException.Conflict("La denuncia no se encuentra en un estado resoluble.");
        }

        if (!Enum.IsDefined(typeof(TipoContenidoDenuncia), denuncia.TipoContenido))
            throw AppException.BadRequest("El tipo de contenido denunciado no es válido.");

        var resultado = (dto.Resultado ?? string.Empty).Trim();

        if (resultado.Length == 0)
            throw AppException.BadRequest("El resultado de la resolución es obligatorio.");

        if (resultado.Length > 500)
            throw AppException.BadRequest("El resultado no puede superar los 500 caracteres.");

        var ahora = DateTime.Now;
        var estadoAnterior = denuncia.Estado;

        using var scope = CrearTransaccion();

        if (dto.OcultarContenido)
        {
            var existeContenido = await _repo.ExisteContenidoAsync(
                denuncia.TipoContenido,
                denuncia.IdContenido);

            if (!existeContenido)
                throw AppException.NotFound("El contenido denunciado no existe.");

            await _repo.OcultarContenidoAsync(
                denuncia.TipoContenido,
                denuncia.IdContenido,
                ahora);
        }

        var resuelta = await _repo.MarcarDenunciaResueltaAsync(denunciaId, resultado, ahora);

        if (!resuelta)
            throw AppException.Conflict("La denuncia ya fue resuelta por otra solicitud.");

        await _trazabilidad.InsertarAuditoriaAsync(CrearAuditoria(
            adminId,
            "ResolverDenuncia",
            "Denuncias",
            denunciaId,
            $"Estado={NombreEstadoDenuncia(estadoAnterior)}",
            $"Estado={EstadoDenuncia.Resuelta}; OcultarContenido={dto.OcultarContenido}; Resultado={resultado}",
            ahora));

        scope.Complete();
    }

    // ADM-03: Configuración

    public async Task<IEnumerable<ConfiguracionDTO>> ObtenerConfiguracionesAsync()
    {
        var configuraciones = await _repo.ObtenerConfiguracionesAsync();

        return configuraciones
            .Select(c => c.ToDto(EsClaveSensible(c.Clave)))
            .ToList();
    }

    public async Task<int> CrearConfiguracionAsync(CrearConfiguracionDTO dto, int adminId)
    {
        var clave = ValidarClave(dto.Clave);
        var valor = ValidarValor(dto.Valor);
        var descripcion = ValidarDescripcion(dto.Descripcion);

        if (EsClaveSensible(clave))
            throw AppException.BadRequest("Las claves sensibles no se administran desde este módulo.");

        if (await _repo.ExisteClaveConfiguracionAsync(clave, null))
            throw AppException.Conflict("Ya existe una configuración con esa clave.");

        var ahora = DateTime.Now;
        var configuracion = new ConfiguracionSistema
        {
            Clave = clave,
            Valor = valor,
            Descripcion = descripcion,
            Activo = true
        };

        using var scope = CrearTransaccion();

        await _repo.InsertarConfiguracionAsync(configuracion);

        await _trazabilidad.InsertarAuditoriaAsync(CrearAuditoria(
            adminId,
            "CrearConfiguracion",
            "ConfiguracionSistema",
            configuracion.ConfiguracionId,
            null,
            $"Clave={clave}; Descripcion={descripcion ?? "N/A"}",
            ahora));

        scope.Complete();
        return configuracion.ConfiguracionId;
    }

    public async Task ActualizarConfiguracionAsync(
        int configuracionId,
        CrearConfiguracionDTO dto,
        int adminId)
    {
        var configuracion = await _repo.ObtenerConfiguracionAsync(configuracionId)
            ?? throw AppException.NotFound("La configuración no existe.");

        var clave = ValidarClave(dto.Clave);
        var valor = ValidarValor(dto.Valor);
        var descripcion = ValidarDescripcion(dto.Descripcion);

        if (EsClaveSensible(clave) || EsClaveSensible(configuracion.Clave))
            throw AppException.BadRequest("Las claves sensibles no se administran desde este módulo.");

        if (await _repo.ExisteClaveConfiguracionAsync(clave, configuracionId))
            throw AppException.Conflict("Ya existe una configuración con esa clave.");

        var ahora = DateTime.Now;
        var valorAnterior = $"Clave={configuracion.Clave}; Valor={configuracion.Valor}; Descripcion={configuracion.Descripcion ?? "N/A"}";

        using var scope = CrearTransaccion();

        configuracion.Clave = clave;
        configuracion.Valor = valor;
        configuracion.Descripcion = descripcion;
        configuracion.FechaModificacion = ahora;

        await _repo.ActualizarConfiguracionAsync(configuracion);

        await _trazabilidad.InsertarAuditoriaAsync(CrearAuditoria(
            adminId,
            "ActualizarConfiguracion",
            "ConfiguracionSistema",
            configuracionId,
            valorAnterior,
            $"Clave={clave}; Valor={valor}; Descripcion={descripcion ?? "N/A"}",
            ahora));

        scope.Complete();
    }

    // ADM-04: Dashboard

    public async Task<DashboardMetricasDTO> ObtenerMetricasAsync()
    {
        // Se ejecutan secuencialmente sobre el mismo DbContext.
        var adopciones = await _repo.ContarAdopcionesExitosasAsync();
        var mascotas = await _repo.ContarMascotasDisponiblesAsync();
        var usuarios = await _repo.ContarUsuariosActivosAsync();
        var refugios = await _repo.ContarRefugiosVerificadosAsync();
        var reportes = await _repo.ContarReportesCiudadanosAsync();
        var promedio = await _repo.ObtenerTiempoPromedioAdopcionDiasAsync();

        return new DashboardMetricasDTO
        {
            TotalAdopcionesExitosas = adopciones,
            TotalMascotasDisponibles = mascotas,
            TotalUsuariosRegistrados = usuarios,
            TotalRefugiosVerificados = refugios,
            TotalReportesCiudadanos = reportes,
            TiempoPromedioAdopcionDias = Math.Round(promedio, 1)
        };
    }

    // Helpers

    private static TransactionScope CrearTransaccion()
    {
        return new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);
    }

    private static RegistrosAuditorias CrearAuditoria(
        int adminId,
        string accion,
        string entidadAfectada,
        int? idEntidad,
        string? valorAnterior,
        string? valorNuevo,
        DateTime fecha)
    {
        return new RegistrosAuditorias
        {
            UsuarioId = adminId > 0 ? adminId : null,
            Accion = accion,
            EntidadAfectada = entidadAfectada,
            IdEntidad = idEntidad,
            ValorAnterior = valorAnterior,
            ValorNuevo = valorNuevo,
            FechaCreacion = fecha
        };
    }

    private static string ValidarClave(string? clave)
    {
        var valor = clave?.Trim() ?? string.Empty;

        if (valor.Length == 0)
            throw AppException.BadRequest("La clave de configuración es obligatoria.");

        if (valor.Length > 80)
            throw AppException.BadRequest("La clave no puede superar los 80 caracteres.");

        return valor;
    }

    private static string ValidarValor(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw AppException.BadRequest("El valor de configuración es obligatorio.");

        if (valor.Length > 400)
            throw AppException.BadRequest("El valor no puede superar los 400 caracteres.");

        return valor;
    }

    private static string? ValidarDescripcion(string? descripcion)
    {
        var valor = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();

        if (valor is { Length: > 300 })
            throw AppException.BadRequest("La descripción no puede superar los 300 caracteres.");

        return valor;
    }

    private static bool EsClaveSensible(string clave)
    {
        return FragmentosClaveSensible.Any(fragmento =>
            clave.Contains(fragmento, StringComparison.OrdinalIgnoreCase));
    }

    private static string NombreEstadoUsuario(byte valor)
    {
        return Enum.IsDefined(typeof(EstadoUsuario), valor)
            ? ((EstadoUsuario)valor).ToString()
            : valor.ToString();
    }

    private static string NombreEstadoDenuncia(byte valor)
    {
        return Enum.IsDefined(typeof(EstadoDenuncia), valor)
            ? ((EstadoDenuncia)valor).ToString()
            : valor.ToString();
    }
}
