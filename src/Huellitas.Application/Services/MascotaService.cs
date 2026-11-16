using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Mascotas;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Mappings;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Services;

public class MascotaService : IMascotaService
{
    private readonly IMascotaRepository _mascotaRepo;
    private readonly IRefugioRepository _refugioRepo;
    private readonly IRegistroClinicoRepository _registroClinicoRepo;

    public MascotaService(IMascotaRepository mascotaRepo, IRefugioRepository refugioRepo, IRegistroClinicoRepository registroClinicoRepo)
    {
        _mascotaRepo = mascotaRepo;
        _refugioRepo = refugioRepo;
        _registroClinicoRepo = registroClinicoRepo;
    }

    public async Task<int> RegistrarAsync(CrearMascotaDTO dto, int usuarioId)
    {
        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("El usuario no está asociado a un refugio.");

        if (refugio.EstadoVerificacion != 1)
            throw AppException.Forbidden("Solo los refugios verificados pueden publicar mascotas.");

        //Usamos el mapeo y luego completamos lo que es lógica de negocio
        var mascota = dto.ToEntity();

        mascota.RefugioId = refugio.RefugioId;
        mascota.Estado = (byte)EstadoMascota.Disponible;
        mascota.FechaIngreso = DateOnly.FromDateTime(DateTime.Now);
        mascota.FechaUltimaActualizacion = DateTime.Now;
        mascota.Activo = true;
        mascota.FechaCreacion = DateTime.Now;

        await _mascotaRepo.InsertarAsync(mascota);

        // Insertar fotos y, si existe, el enlace del video
        var (fotos, videos) = SepararMultimedia(dto.FotosUrls, dto.VideoUrl);
        await GuardarMultimediaAsync(mascota.MascotaId, fotos, videos);

        await _mascotaRepo.InsertarBitacoraAsync(new Bitacoras
        {
            MascotaId = mascota.MascotaId,
            Tipo = (byte)TipoBitacora.Ingreso,
            Descripcion = "Mascota registrada en el sistema.",
            FechaCreacion = DateTime.Now
        });

        return mascota.MascotaId;
    }


    public async Task ActualizarAsync(int mascotaId, ActualizarMascotaDTO dto, int usuarioId)
    {
        var mascota = await ValidarRefugioAsync(mascotaId, usuarioId);

        mascota.UpdateFromDto(dto);
        mascota.FechaUltimaActualizacion = DateTime.Now;

        await _mascotaRepo.ActualizarAsync(mascota);

        // Si envían nuevas fotos o video, reemplazamos la multimedia anterior
        var (fotos, videos) = SepararMultimedia(dto.FotosUrls, dto.VideoUrl);
        if (fotos.Count > 0 || videos.Count > 0)
        {
            await _mascotaRepo.EliminarMultimediaAsync(mascotaId);
            await GuardarMultimediaAsync(mascotaId, fotos, videos);
        }
    }

    /// <summary>Separa los enlaces recibidos en fotos y videos.</summary>
    private static (List<string> Fotos, List<string> Videos) SepararMultimedia(
        IEnumerable<string>? urls, string? videoUrl)
    {
        var lista = urls?.Where(u => !string.IsNullOrWhiteSpace(u)).ToList() ?? new();
        var fotos = lista.Where(u => !MultimediaHelper.EsVideo(u)).ToList();
        var videos = lista.Where(MultimediaHelper.EsVideo).ToList();

        if (!string.IsNullOrWhiteSpace(videoUrl))
        {
            var video = videoUrl.Trim();
            if (!videos.Contains(video))
                videos.Add(video);
        }

        return (fotos, videos);
    }

    /// <summary>Guarda las fotos y el enlace de video de una mascota.</summary>
    private async Task GuardarMultimediaAsync(int mascotaId, IEnumerable<string> fotos, IEnumerable<string> videos)
    {
        var esPrimera = true;
        foreach (var url in fotos)
        {
            await _mascotaRepo.InsertarMultimediaAsync(new Multimedia
            {
                MascotaId = mascotaId,
                Tipo = MultimediaHelper.TipoFoto,
                Archivo = url,
                EsPrincipal = esPrimera,
                FechaCarga = DateTime.Now,
                Activo = true
            });
            esPrimera = false;
        }

        foreach (var url in videos)
        {
            await _mascotaRepo.InsertarMultimediaAsync(new Multimedia
            {
                MascotaId = mascotaId,
                Tipo = MultimediaHelper.TipoVideo,
                Archivo = url,
                EsPrincipal = false,
                FechaCarga = DateTime.Now,
                Activo = true
            });
        }
    }

    public async Task CambiarEstadoAsync(int mascotaId, CambiarEstadoMascotaDTO dto, int usuarioId)
    {
        var mascota = await ValidarRefugioAsync(mascotaId, usuarioId);

        if (dto.NuevoEstado == 3 && string.IsNullOrWhiteSpace(dto.Motivo))
            throw AppException.BadRequest("El motivo es obligatorio al marcar como No Disponible.");

        byte estadoAnterior = mascota.Estado;
        mascota.Estado = dto.NuevoEstado;
        mascota.MotivoBaja = dto.NuevoEstado == 3 ? dto.Motivo : null;
        mascota.FechaUltimaActualizacion = DateTime.Now;

        await _mascotaRepo.ActualizarAsync(mascota);

        await _mascotaRepo.InsertarBitacoraAsync(new Bitacoras
        {
            MascotaId = mascota.MascotaId,
            Tipo = (byte)TipoBitacora.CambioEstado,
            Descripcion = $"Estado cambiado de {estadoAnterior} a {dto.NuevoEstado}. Motivo: {dto.Motivo ?? "N/A"}",
            FechaCreacion = DateTime.Now
        });
    }

    public async Task DarDeBajaAsync(int mascotaId, string motivo, int usuarioId)
    {
        var mascota = await ValidarRefugioAsync(mascotaId, usuarioId);

        if (await _mascotaRepo.TieneSolicitudesActivasAsync(mascotaId))
            throw AppException.Conflict("No se puede dar de baja la mascota porque tiene solicitudes activas.");

        mascota.Activo = false;
        mascota.Estado = 3; // No Disponible
        mascota.MotivoBaja = motivo;
        mascota.FechaUltimaActualizacion = DateTime.Now;

        await _mascotaRepo.ActualizarAsync(mascota);
    }

    public async Task<PagedResult<MascotaResumenDTO>> ObtenerMisMascotasAsync(int usuarioId, int pagina, int tamano)
    {
        var (paginaNormalizada, tamanoNormalizado) = Paginacion.Normalizar(pagina, tamano);

        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("El usuario no está asociado a un refugio.");

        var (items, total) = await _mascotaRepo.ObtenerMisMascotasAsync(refugio.RefugioId, paginaNormalizada, tamanoNormalizado);

        return new PagedResult<MascotaResumenDTO>
        {
            Items = items.ToResumenDtoList().ToList(),
            Pagina = paginaNormalizada,
            TamanoPagina = tamanoNormalizado,
            TotalRegistros = total
        };
    }

    // Método auxiliar para verificar que el refugio es dueño de la mascota
    private async Task<Mascotas> ValidarRefugioAsync(int mascotaId, int usuarioId)
    {
        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("Usuario no asociado a un refugio.");

        var mascota = await _mascotaRepo.ObtenerPorIdAsync(mascotaId)
            ?? throw AppException.NotFound("Mascota no encontrada.");

        if (mascota.RefugioId != refugio.RefugioId)
            throw AppException.Forbidden("No tienes permiso para modificar esta mascota.");

        return mascota;
    }

    public async Task<PagedResult<MascotaResumenDTO>> ObtenerCatalogoAsync(FiltroMascotaDTO filtro)
    {
        var (pagina, tamano) = Paginacion.Normalizar(filtro.Pagina, filtro.TamanoPagina);

        // El servicio extrae los valores del DTO y se los pasa al repo
        var (items, total) = await _mascotaRepo.ObtenerCatalogoPublicoAsync(
            pagina,
            tamano,
            filtro.EspecieId,
            filtro.TamanoId,
            filtro.Sexo);

        return new PagedResult<MascotaResumenDTO>
        {
            Items = items.ToResumenDtoList().ToList(),
            Pagina = pagina,
            TamanoPagina = tamano,
            TotalRegistros = total
        };
    }

    public async Task<MascotaDetalleDTO> ObtenerDetalleAsync(int mascotaId)
    {
        var mascota = await _mascotaRepo.ObtenerDetallePublicoAsync(mascotaId)
            ?? throw AppException.NotFound("La mascota no existe o no está disponible.");

        return mascota.ToDetalleDto();
    }

    public async Task AgregarRegistroClinicoAsync(int mascotaId, CrearRegistroClinicoDTO dto, int usuarioId)
    {
        // Reutilizamos tu método existente (ValidarRefugioAsync)
        var mascota = await ValidarRefugioAsync(mascotaId, usuarioId);

        // Crear el registro usando el nombre correcto de la entidad
        var registro = new RegistrosClinico
        {
            MascotaId = mascotaId,
            Tipo = dto.Tipo,
            Fecha = dto.Fecha,
            Descripcion = dto.Descripcion,
            SoporteVeterinario = dto.SoporteVeterinario,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        await _registroClinicoRepo.InsertarAsync(registro);

        // 3. Registrar en la bitácora
        await _mascotaRepo.InsertarBitacoraAsync(new Bitacoras
        {
            MascotaId = mascotaId,
            Tipo = (byte)TipoBitacora.AtencionClinica,
            Descripcion = $"Registro clínico agregado: {((TipoRegistroClinico)dto.Tipo).ToString()}",
            FechaCreacion = DateTime.Now
        });
    }

    public async Task<PagedResult<RegistroClinicoDTO>> ObtenerHistorialClinicoAsync(int mascotaId, int pagina, int tamano)
    {
        var (paginaNormalizada, tamanoNormalizado) = Paginacion.Normalizar(pagina, tamano);

        // Validamos que la mascota exista
        _ = await _mascotaRepo.ObtenerPorIdAsync(mascotaId)
            ?? throw AppException.NotFound("Mascota no encontrada.");

        // Traemos los datos paginados desde el repositorio
        var (items, total) = await _registroClinicoRepo.ObtenerPorMascotaAsync(mascotaId, paginaNormalizada, tamanoNormalizado);

        // Retornamos el PagedResult mapeado a DTOs
        return new PagedResult<RegistroClinicoDTO>
        {
            Items = items.Select(r => r.ToDto()).ToList(),
            Pagina = paginaNormalizada,
            TamanoPagina = tamanoNormalizado,
            TotalRegistros = total
        };
    }
}