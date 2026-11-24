using System.Transactions;
using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Mascotas;
using Huellitas.Application.DTOs.Reportes;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Mappings;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Services;

public class ReporteService : IReporteService
{
    private const byte ProcedenciaReporteCiudadano = 2;

    private readonly IReporteRepository _reporteRepo;
    private readonly IRefugioRepository _refugioRepo;
    private readonly IMascotaService _mascotaService;
    private readonly ICatalogoRepository _catalogoRepo;

    public ReporteService(
        IReporteRepository reporteRepo,
        IRefugioRepository refugioRepo,
        IMascotaService mascotaService,
        ICatalogoRepository catalogoRepo)
    {
        _reporteRepo = reporteRepo;
        _refugioRepo = refugioRepo;
        _mascotaService = mascotaService;
        _catalogoRepo = catalogoRepo;
    }

    public async Task<ReporteCreadoDTO> CrearReporteAsync(CrearReporteDTO dto, int usuarioId)
    {
        // La validación de campos del DTO vive en CrearReporteValidator.
        NormalizarFotos(dto);

        // El reporte puede ser anónimo: si no hay usuario autenticado, UsuarioId queda null.
        int? usuarioReportante = usuarioId > 0 ? usuarioId : null;

        var ahora = DateTime.Now;
        var reporte = dto.ToEntity(usuarioReportante, ahora);

        var fotos = dto.FotosUrls
            .Select(url => new ReporteMultimedia
            {
                RutaArchivo = url,
                Activo = true
            })
            .ToList();

        var cambioInicial = CrearCambio(
            reporteId: 0,
            usuarioReportante,
            EstadoReporte.Recibido,
            estadoAnterior: null,
            observacion: "Reporte creado.",
            fecha: ahora);

        var creado = await _reporteRepo.InsertarAsync(reporte, fotos, cambioInicial);

        return new ReporteCreadoDTO
        {
            ReporteId = creado.ReporteId,
            NumeroReporte = creado.NumeroReporte
        };
    }

    public async Task<PagedResult<ReporteResumenDTO>> ObtenerBandejaRefugioAsync(
        int usuarioId,
        int pagina,
        int tamano)
    {
        var (paginaValida, tamanoValido) = Paginacion.Validar(pagina, tamano);
        var refugio = await ObtenerRefugioActivoAsync(usuarioId);

        var (items, total) = await _reporteRepo.ObtenerBandejaAsync(
            refugio.RefugioId,
            paginaValida,
            tamanoValido);

        return new PagedResult<ReporteResumenDTO>
        {
            Items = items.Select(r => r.ToResumenDto(refugio.RefugioId)).ToList(),
            Pagina = paginaValida,
            TamanoPagina = tamanoValido,
            TotalRegistros = total
        };
    }

    public async Task<ReporteDetallePublicoDTO> ObtenerDetalleAsync(
        int reporteId,
        int usuarioId,
        bool esAdmin)
    {
        var reporte = await _reporteRepo.ObtenerDetalleAsync(reporteId)
            ?? throw AppException.NotFound("El reporte no existe.");

        if (!reporte.Activo)
            throw AppException.NotFound("El reporte no está disponible.");

        // El reportante autenticado puede ver su propio reporte con el detalle privado.
        var esReportante = reporte.UsuarioId is not null && reporte.UsuarioId == usuarioId;
        var esRefugioAsignado = await EsRefugioAsignadoAsync(reporte, usuarioId);

        if (esAdmin || esReportante || esRefugioAsignado)
            return reporte.ToDetalleDto();

        // Un usuario público solo puede ver reportes en estados visibles.
        if (!EsVisiblePublicamente((EstadoReporte)reporte.Estado))
            throw AppException.NotFound("El reporte no está disponible.");

        return reporte.ToPublicoDto();
    }

    public async Task TomarReporteAsync(int reporteId, int usuarioId, bool esAdmin)
    {
        int? refugioId;
        if (esAdmin)
            refugioId = await ObtenerRefugioIdOpcionalAsync(usuarioId);
        else
            refugioId = (await ObtenerRefugioActivoAsync(usuarioId)).RefugioId;

        var reporte = await _reporteRepo.ObtenerBasicoAsync(reporteId)
            ?? throw AppException.NotFound("El reporte no existe.");

        if (!reporte.Activo)
            throw AppException.NotFound("El reporte no está disponible.");

        if (reporte.RefugioId is not null)
        {
            throw !esAdmin && reporte.RefugioId == refugioId
                ? AppException.Conflict("El reporte ya está asignado a tu refugio.")
                : AppException.Conflict("El reporte ya fue tomado por otro refugio.");
        }

        if ((EstadoReporte)reporte.Estado != EstadoReporte.Recibido)
            throw AppException.Conflict("Solo se pueden tomar reportes en estado Recibido.");

        var ahora = DateTime.Now;
        var observacion = esAdmin && refugioId is null
            ? "Reporte tomado por un administrador."
            : "Reporte tomado por el refugio.";

        var cambio = CrearCambio(
            reporteId,
            usuarioId,
            EstadoReporte.EnAtencion,
            EstadoReporte.Recibido,
            observacion,
            ahora);

        var tomado = await _reporteRepo.TomarAsync(reporteId, refugioId, cambio);

        if (!tomado)
            throw AppException.Conflict("El reporte ya fue tomado por otro refugio. Actualiza la bandeja.");
    }

    public async Task CambiarEstadoAsync(
        int reporteId,
        CambiarEstadoReporteDTO dto,
        int usuarioId,
        bool esAdmin)
    {
        // La validación de campos del DTO vive en CambiarEstadoReporteValidator.
        var motivo = string.IsNullOrWhiteSpace(dto.Motivo) ? null : dto.Motivo.Trim();
        var nuevoEstado = (EstadoReporte)dto.NuevoEstado;

        var reporte = await _reporteRepo.ObtenerBasicoAsync(reporteId)
            ?? throw AppException.NotFound("El reporte no existe.");

        if (!reporte.Activo)
            throw AppException.NotFound("El reporte no está disponible.");

        int? refugioId = null;
        if (!esAdmin)
        {
            var refugio = await ObtenerRefugioActivoAsync(usuarioId);

            if (reporte.RefugioId != refugio.RefugioId)
                throw AppException.Forbidden("Solo el refugio asignado puede cambiar el estado del reporte.");

            refugioId = refugio.RefugioId;
        }

        if (reporte.MascotaId is not null)
            throw AppException.Conflict("El reporte ya fue convertido en mascota y no admite cambios de estado.");

        var estadoActual = (EstadoReporte)reporte.Estado;

        if (estadoActual == nuevoEstado)
            throw AppException.Conflict("El reporte ya se encuentra en ese estado.");

        if (!EsTransicionValida(estadoActual, nuevoEstado))
            throw AppException.Conflict($"No se permite pasar de {estadoActual} a {nuevoEstado}.");

        var motivoCierre = RequiereMotivo(nuevoEstado) ? motivo : null;

        var ahora = DateTime.Now;
        var cambio = CrearCambio(
            reporteId,
            usuarioId,
            nuevoEstado,
            estadoActual,
            motivo ?? "Cambio de estado del reporte.",
            ahora);

        var actualizado = await _reporteRepo.CambiarEstadoAsync(
            reporteId,
            refugioId,
            reporte.Estado,
            dto.NuevoEstado,
            motivoCierre,
            cambio);

        if (!actualizado)
            throw AppException.Conflict("El estado del reporte cambió. Actualiza la información e intenta nuevamente.");
    }

    public async Task<int> ConvertirAFichaMascotaAsync(
        int reporteId,
        ConvertirMascotaDTO dto,
        int usuarioId)
    {
        // La validación de campos del DTO vive en ConvertirMascotaValidator.
        var refugio = await ObtenerRefugioActivoAsync(usuarioId);
        var reporte = await _reporteRepo.ObtenerParaConversionAsync(reporteId)
            ?? throw AppException.NotFound("El reporte no existe.");

        if (!reporte.Activo)
            throw AppException.NotFound("El reporte no está disponible.");

        if (reporte.RefugioId != refugio.RefugioId)
            throw AppException.Forbidden("Solo el refugio asignado puede convertir el reporte en mascota.");

        if ((EstadoReporte)reporte.Estado != EstadoReporte.Rescatado)
            throw AppException.Conflict("Solo se pueden convertir reportes en estado Rescatado.");

        if (reporte.MascotaId is not null)
            throw AppException.Conflict("El reporte ya tiene una mascota vinculada.");

        await ValidarCatalogosAsync(dto);

        var crearMascota = new CrearMascotaDTO
        {
            Nombre = dto.Nombre.Trim(),
            EspecieId = dto.EspecieId,
            RazaId = dto.RazaId,
            TamanoId = dto.TamanoId,
            ColorId = dto.ColorId,
            Sexo = dto.Sexo,
            EdadAproximada = dto.EdadAproximada,
            Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion)
                ? reporte.Descripcion
                : dto.Descripcion.Trim(),
            Esterilizada = dto.Esterilizada,
            Procedencia = ProcedenciaReporteCiudadano,
            // Se reutilizan las URLs del reporte, pero MascotaService crea sus propias
            // filas de Multimedia. Compartir la misma fila (ReporteId + MascotaId) haría
            // que EliminarMultimediaAsync, al editar fotos de la mascota, borrara las del reporte.
            FotosUrls = reporte.Fotos?
                .Where(f => f.Activo)
                .OrderBy(f => f.Id)
                .Select(f => f.RutaArchivo)
                .ToList() ?? new List<string>()
        };

        // La creación reutiliza MascotaService. La transacción ambiente garantiza que
        // el alta de la mascota y el vínculo con el reporte sean atómicos: si el vínculo
        // condicional falla (otra solicitud ganó la carrera), la mascota se revierte.
        using var scope = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);

        var mascotaId = await _mascotaService.RegistrarAsync(crearMascota, usuarioId);

        var cambio = CrearCambio(
            reporteId,
            usuarioId,
            EstadoReporte.Rescatado,
            EstadoReporte.Rescatado,
            $"Reporte convertido en mascota (MascotaId {mascotaId}).",
            DateTime.Now);

        var vinculado = await _reporteRepo.VincularMascotaAsync(
            reporteId,
            refugio.RefugioId,
            mascotaId,
            cambio);

        if (!vinculado)
            throw AppException.Conflict("El reporte ya fue convertido en mascota por otra solicitud.");

        scope.Complete();

        return mascotaId;
    }

    public async Task<IEnumerable<ReporteMapaDTO>> ObtenerReportesMapaAsync()
    {
        var reportes = await _reporteRepo.ObtenerMapaAsync();
        return reportes.Select(r => r.ToMapaDto()).ToList();
    }

    private async Task<Refugios> ObtenerRefugioActivoAsync(int usuarioId)
    {
        if (usuarioId <= 0)
            throw AppException.Unauthorized("Debes iniciar sesión.");

        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("El usuario no está asociado a un refugio.");

        if (!refugio.Activo)
            throw AppException.Forbidden("El refugio no está activo.");

        return refugio;
    }

    /// <summary>
    /// Refugio del administrador, si tiene uno activo. Un administrador sin refugio
    /// puede operar sobre los reportes sin asignarles refugio.
    /// </summary>
    private async Task<int?> ObtenerRefugioIdOpcionalAsync(int usuarioId)
    {
        if (usuarioId <= 0)
            return null;

        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId);
        return refugio is { Activo: true } ? refugio.RefugioId : null;
    }

    private async Task<bool> EsRefugioAsignadoAsync(ReportesCiudadano reporte, int usuarioId)
    {
        if (usuarioId <= 0 || reporte.RefugioId is null)
            return false;

        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId);
        return refugio is not null && refugio.RefugioId == reporte.RefugioId;
    }

    private static void NormalizarFotos(CrearReporteDTO dto)
    {
        if (dto.FotosUrls is null)
        {
            dto.FotosUrls = new List<string>();
            return;
        }

        dto.FotosUrls = dto.FotosUrls
            .Select(url => url.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task ValidarCatalogosAsync(ConvertirMascotaDTO dto)
    {
        var especies = await _catalogoRepo.ObtenerEspeciesAsync();
        if (!especies.Any(e => e.EspecieId == dto.EspecieId))
            throw AppException.BadRequest("La especie indicada no existe o no está activa.");

        if (dto.RazaId.HasValue)
        {
            var razas = await _catalogoRepo.ObtenerRazasPorEspecieAsync(dto.EspecieId);
            if (!razas.Any(r => r.RazaId == dto.RazaId.Value))
                throw AppException.BadRequest("La raza indicada no existe, no está activa o no pertenece a la especie.");
        }

        var tamanos = await _catalogoRepo.ObtenerTamanosAsync();
        if (!tamanos.Any(t => t.TamanoId == dto.TamanoId))
            throw AppException.BadRequest("El tamaño indicado no existe o no está activo.");

        var colores = await _catalogoRepo.ObtenerColoresAsync();
        if (!colores.Any(c => c.ColorId == dto.ColorId))
            throw AppException.BadRequest("El color indicado no existe o no está activo.");
    }

    // Reglas del flujo de reportes. Se mantienen privadas del servicio porque no son
    // reglas compartidas a nivel de capa.
    private static bool EsTransicionValida(EstadoReporte actual, EstadoReporte nuevo)
    {
        return actual == EstadoReporte.EnAtencion &&
               nuevo is EstadoReporte.Rescatado or EstadoReporte.NoEncontrado or EstadoReporte.Cerrado;
    }

    private static bool RequiereMotivo(EstadoReporte nuevo)
    {
        return nuevo is EstadoReporte.NoEncontrado or EstadoReporte.Cerrado;
    }

    private static bool EsVisiblePublicamente(EstadoReporte estado)
    {
        return estado is EstadoReporte.Recibido or EstadoReporte.EnAtencion or EstadoReporte.Rescatado;
    }

    private static CambiosDeEstado CrearCambio(
        int reporteId,
        int? usuarioId,
        EstadoReporte estadoNuevo,
        EstadoReporte? estadoAnterior,
        string observacion,
        DateTime fecha)
    {
        return new CambiosDeEstado
        {
            UsuarioId = usuarioId,
            Entidad = EntidadAfectada.ReporteCiudadano.ToString(),
            IdEntidad = reporteId,
            EstadoAnterior = estadoAnterior?.ToString(),
            EstadoNuevo = estadoNuevo.ToString(),
            Observacion = observacion,
            FechaCreacion = fecha
        };
    }
}
