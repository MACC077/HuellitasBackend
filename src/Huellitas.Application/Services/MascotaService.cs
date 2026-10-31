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

    public MascotaService(IMascotaRepository mascotaRepo, IRefugioRepository refugioRepo)
    {
        _mascotaRepo = mascotaRepo;
        _refugioRepo = refugioRepo;
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

        // Insertar fotos
        bool esPrimera = true;
        foreach (var url in dto.FotosUrls)
        {
            await _mascotaRepo.InsertarMultimediaAsync(new Multimedia
            {
                MascotaId = mascota.MascotaId,
                Tipo = 0,
                Archivo = url,
                EsPrincipal = esPrimera,
                FechaCarga = DateTime.Now,
                Activo = true
            });
            esPrimera = false;
        }

        await _mascotaRepo.InsertarBitacoraAsync(new Bitacoras
        {
            MascotaId = mascota.MascotaId,
            Tipo = 0,
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

        // Si envían nuevas fotos, reemplazamos las anteriores
        if (dto.FotosUrls != null && dto.FotosUrls.Any())
        {
            await _mascotaRepo.EliminarMultimediaAsync(mascotaId);
            bool esPrimera = true;
            foreach (var url in dto.FotosUrls)
            {
                await _mascotaRepo.InsertarMultimediaAsync(new Multimedia
                {
                    MascotaId = mascotaId,
                    Tipo = 0,
                    Archivo = url,
                    EsPrincipal = esPrimera,
                    FechaCarga = DateTime.Now,
                    Activo = true
                });
                esPrimera = false;
            }
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
            Tipo = 1, // Cambio de estado
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
        var refugio = await _refugioRepo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.BadRequest("El usuario no está asociado a un refugio.");

        var (items, total) = await _mascotaRepo.ObtenerMisMascotasAsync(refugio.RefugioId, pagina, tamano);

        return new PagedResult<MascotaResumenDTO>
        {
            Items = items.ToResumenDtoList().ToList(),
            Pagina = pagina,
            TamanoPagina = tamano,
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
        // El servicio extrae los valores del DTO y se los pasa al repo
        var (items, total) = await _mascotaRepo.ObtenerCatalogoPublicoAsync(
            filtro.Pagina,
            filtro.TamanoPagina,
            filtro.EspecieId,
            filtro.TamanoId,
            filtro.Sexo);

        return new PagedResult<MascotaResumenDTO>
        {
            Items = items.ToResumenDtoList().ToList(),
            Pagina = filtro.Pagina,
            TamanoPagina = filtro.TamanoPagina,
            TotalRegistros = total
        };
    }

    public async Task<MascotaDetalleDTO> ObtenerDetalleAsync(int mascotaId)
    {
        var mascota = await _mascotaRepo.ObtenerDetallePublicoAsync(mascotaId)
            ?? throw AppException.NotFound("La mascota no existe o no está disponible.");

        return mascota.ToDetalleDto();
    }
}