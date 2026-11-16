using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Refugios;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Mappings;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Services;

public class RefugioService : IRefugioService
{
    private readonly IRefugioRepository _repo;
    private readonly IUsuarioRepository _usuarioRepo;
    public RefugioService(IRefugioRepository repo, IUsuarioRepository usuarioRepo)
    {
        _repo = repo;
        _usuarioRepo = usuarioRepo;
    }

    public async Task<PerfilRefugioDTO> ObtenerMiPerfilAsync(int usuarioId)
    {
        var refugio = await _repo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.NotFound("No tienes un refugio asociado.");
        return refugio.ToPerfilDto();
    }

    public async Task ActualizarPerfilAsync(int usuarioId, ActualizarRefugioDTO dto)
    {
        var refugio = await _repo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.NotFound("No tienes un refugio asociado.");

        refugio.Descripcion = dto.Descripcion;
        refugio.Direccion = dto.Direccion;
        refugio.Telefono = dto.Telefono;
        refugio.RedesSociales = RefugioMappings.SerializarRedesSociales(dto.RedesSociales);
        refugio.FechaModificacion = DateTime.Now;

        await _repo.ActualizarAsync(refugio);
    }

    public async Task AgregarRequisitoAsync(int usuarioId, RequisitoDTO dto)
    {
        var refugio = await _repo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.NotFound("No tienes un refugio asociado.");

        var requisito = dto.ToEntity(refugio.RefugioId);
        await _repo.InsertarRequisitoAsync(requisito);
    }

    public async Task EliminarRequisitoAsync(int usuarioId, int requisitoId)
    {
        var refugio = await _repo.ObtenerPorUsuarioIdAsync(usuarioId)
            ?? throw AppException.NotFound("No tienes un refugio asociado.");

        var eliminado = await _repo.EliminarRequisitoAsync(requisitoId, refugio.RefugioId);
        if (!eliminado)
            throw AppException.NotFound("Requisito no encontrado.");
    }

    public async Task VerificarRefugioAsync(int refugioId, VerificarRefugioDTO dto)
    {
        var refugio = await _repo.ObtenerPorIdAsync(refugioId)
            ?? throw AppException.NotFound("Refugio no encontrado.");

        if (dto.Aprobado)
        {
            refugio.EstadoVerificacion = (byte)EstadoVerificacionRefugio.Verificado;
            refugio.FechaVerificacion = DateTime.Now;
            refugio.MotivoRechazo = null;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.MotivoRechazo))
                throw AppException.BadRequest("El motivo de rechazo es obligatorio.");

            refugio.EstadoVerificacion = (byte)EstadoVerificacionRefugio.Rechazado; // Rechazado
            refugio.MotivoRechazo = dto.MotivoRechazo;
        }

        await _repo.ActualizarAsync(refugio);
        
        //Logica de Rechazo
        var usuario = await _usuarioRepo.ObtenerPorRefugioIdAsync(refugioId);
        if (usuario != null)
        {
            // 1. Desvincular el refugio del usuario
            usuario.RefugioId = null;
            // 2. Devolverle su rol a Adoptante
            usuario.RolId = (int)RolUsuario.Adoptante;
            usuario.FechaModificacion = DateTime.Now;

            await _usuarioRepo.ActualizarAsync(usuario);
        }

    }

    public async Task<PagedResult<RefugioResumenDTO>> ObtenerRefugiosPublicosAsync(int pagina, int tamano)
    {
        var (paginaNormalizada, tamanoNormalizado) = Paginacion.Normalizar(pagina, tamano);
        var (items, total) = await _repo.ObtenerRefugiosPublicosAsync(paginaNormalizada, tamanoNormalizado);

        return new PagedResult<RefugioResumenDTO>
        {
            Items = items.Select(r => r.ToResumenDto()).ToList(),
            Pagina = paginaNormalizada,
            TamanoPagina = tamanoNormalizado,
            TotalRegistros = total
        };
    }

    public async Task<RefugioPublicoDTO> ObtenerPerfilPublicoAsync(int refugioId)
    {
        var refugio = await _repo.ObtenerPerfilPublicoPorIdAsync(refugioId)
            ?? throw AppException.NotFound("Refugio no encontrado o no está verificado.");

        return refugio.ToPublicoDto();
    }

    public async Task SolicitarRegistroRefugioAsync(int usuarioId, CrearRefugioDTO dto)
    {
        var usuario = await _usuarioRepo.ObtenerPorIdAsync(usuarioId)
            ?? throw AppException.NotFound("Usuario no encontrado.");

        if (usuario.RefugioId.HasValue)
            throw AppException.Conflict("Ya tienes un refugio asociado a tu cuenta.");

        if (await _repo.ExisteNitAsync(dto.Nit))
            throw AppException.Conflict("Ya existe un refugio registrado con este NIT.");

        var refugio = new Refugios
        {
            Nombre = dto.Nombre,
            Nit = dto.Nit,
            Direccion = dto.Direccion,
            Telefono = dto.Telefono,
            Correo = dto.Correo,
            Encargado = dto.Encargado,
            Descripcion = dto.Descripcion,
            RedesSociales = RefugioMappings.SerializarRedesSociales(dto.RedesSociales), //Serialización JSON
            EstadoVerificacion = (byte)EstadoVerificacionRefugio.Pendiente,
            Activo = true,
            FechaCreacion = DateTime.Now
        };

        await _repo.InsertarAsync(refugio);

        // Asociar el refugio al usuario y cambiar su rol a Refugio
        usuario.RefugioId = refugio.RefugioId;
        usuario.RolId = (int)RolUsuario.Refugio;
        await _usuarioRepo.ActualizarAsync(usuario);
    }
}