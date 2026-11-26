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

        // Aquí idealmente validarías que el requisito pertenece al refugio antes de borrar
        await _repo.EliminarRequisitoAsync(requisitoId);
    }

    public async Task VerificarRefugioAsync(int refugioId, VerificarRefugioDTO dto, int adminId)
    {
        var refugio = await _repo.ObtenerPorIdAsync(refugioId)
            ?? throw AppException.NotFound("Refugio no encontrado.");

        var ahora = DateTime.Now;

        if (dto.Aprobado)
        {
            refugio.EstadoVerificacion = (byte)EstadoVerificacionRefugio.Verificado;
            refugio.FechaVerificacion = ahora;
            refugio.MotivoRechazo = null;
            refugio.VerificadoPorId = adminId > 0 ? adminId : null;
            refugio.FechaModificacion = ahora;

            await _repo.ActualizarAsync(refugio);
            return;
        }

        if (string.IsNullOrWhiteSpace(dto.MotivoRechazo))
            throw AppException.BadRequest("El motivo de rechazo es obligatorio.");

        refugio.EstadoVerificacion = (byte)EstadoVerificacionRefugio.Rechazado;
        refugio.MotivoRechazo = dto.MotivoRechazo.Trim();
        refugio.FechaVerificacion = ahora;
        refugio.VerificadoPorId = adminId > 0 ? adminId : null;
        refugio.FechaModificacion = ahora;

        await _repo.ActualizarAsync(refugio);

        // Solo al rechazar se desvincula al usuario y se le devuelve el rol de Adoptante.
        // Al aprobar, el usuario debe conservar el refugio y su rol.
        var usuario = await _usuarioRepo.ObtenerPorRefugioIdAsync(refugioId);
        if (usuario != null)
        {
            usuario.RefugioId = null;
            usuario.RolId = (int)RolUsuario.Adoptante;
            usuario.FechaModificacion = ahora;

            await _usuarioRepo.ActualizarAsync(usuario);
        }
    }

    public async Task SuspenderRefugioAsync(int refugioId, SuspenderRefugioDTO dto, int adminId)
    {
        var refugio = await _repo.ObtenerPorIdAsync(refugioId)
            ?? throw AppException.NotFound("Refugio no encontrado.");

        var motivo = (dto.Motivo ?? string.Empty).Trim();

        if (motivo.Length == 0)
            throw AppException.BadRequest("El motivo de suspensión es obligatorio.");

        if (refugio.EstadoVerificacion == (byte)EstadoVerificacionRefugio.Suspendido)
            throw AppException.Conflict("El refugio ya está suspendido.");

        var ahora = DateTime.Now;

        refugio.EstadoVerificacion = (byte)EstadoVerificacionRefugio.Suspendido;
        refugio.MotivoRechazo = motivo.Length > 500 ? motivo[..500] : motivo;
        refugio.FechaVerificacion = ahora;
        refugio.VerificadoPorId = adminId > 0 ? adminId : null;
        refugio.FechaModificacion = ahora;

        await _repo.ActualizarAsync(refugio);
    }

    public async Task<PagedResult<RefugioAdminDTO>> ObtenerRefugiosPendientesAsync(int pagina, int tamano)
    {
        var (paginaValida, tamanoValido) = Paginacion.Validar(pagina, tamano);

        var (items, total) = await _repo.ObtenerRefugiosPorEstadoAsync(
            (byte)EstadoVerificacionRefugio.Pendiente,
            paginaValida,
            tamanoValido);

        return new PagedResult<RefugioAdminDTO>
        {
            Items = items.Select(r => r.ToAdminDto()).ToList(),
            Pagina = paginaValida,
            TamanoPagina = tamanoValido,
            TotalRegistros = total
        };
    }

    public async Task<IEnumerable<RefugioSoporteDTO>> ObtenerSoportesAsync(int refugioId)
    {
        var refugio = await _repo.ObtenerPorIdAsync(refugioId)
            ?? throw AppException.NotFound("Refugio no encontrado.");

        return refugio.Soportes?
            .Where(s => s.Activo)
            .OrderBy(s => s.SoporteId)
            .Select(s => new RefugioSoporteDTO
            {
                SoporteId = s.SoporteId,
                Tipo = s.Tipo,
                Archivo = s.Archivo,
                FechaCarga = s.FechaCarga
            }).ToList() ?? new();
    }

    public async Task<PagedResult<RefugioResumenDTO>> ObtenerRefugiosPublicosAsync(int pagina, int tamano)
    {
        var (items, total) = await _repo.ObtenerRefugiosPublicosAsync(pagina, tamano);

        return new PagedResult<RefugioResumenDTO>
        {
            Items = items.Select(r => r.ToResumenDto()).ToList(),
            Pagina = pagina,
            TamanoPagina = tamano,
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

        var soportes = SoportesValidator.Validar(dto.SoportesUrls);

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

        if (soportes.Count > 0)
        {
            await _repo.InsertarSoportesAsync(soportes.Select(url => new RefugioSoportes
            {
                RefugioId = refugio.RefugioId,
                Tipo = SoportesValidator.Tipo(url),
                Archivo = url,
                FechaCarga = DateTime.Now,
                Activo = true
            }));
        }

        // Asociar el refugio al usuario y cambiar su rol a Refugio
        usuario.RefugioId = refugio.RefugioId;
        usuario.RolId = (int)RolUsuario.Refugio;
        await _usuarioRepo.ActualizarAsync(usuario);
    }
}