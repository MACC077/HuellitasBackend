using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Denuncias;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Services;

public class DenunciaService : IDenunciaService
{
    private readonly IDenunciaRepository _repo;

    public DenunciaService(IDenunciaRepository repo) => _repo = repo;

    public async Task<int> CrearAsync(int usuarioId, CrearDenunciaDTO dto)
    {
        if (usuarioId <= 0)
            throw AppException.Unauthorized("Debes iniciar sesión para registrar una denuncia.");

        if (!Enum.IsDefined(typeof(TipoContenidoDenuncia), dto.TipoContenido))
            throw AppException.BadRequest("El tipo de contenido denunciado no es válido.");

        if (!Enum.IsDefined(typeof(MotivoDenuncia), dto.Motivo))
            throw AppException.BadRequest("El motivo de la denuncia no es válido.");

        if (dto.IdContenido <= 0)
            throw AppException.BadRequest("El identificador del contenido denunciado es obligatorio.");

        var descripcion = string.IsNullOrWhiteSpace(dto.Descripcion)
            ? null
            : dto.Descripcion.Trim();

        if (descripcion is { Length: > 1000 })
            throw AppException.BadRequest("La descripción no puede superar los 1000 caracteres.");

        if (!await _repo.ExisteContenidoAsync(dto.TipoContenido, dto.IdContenido))
            throw AppException.NotFound("El contenido denunciado no existe o ya no está disponible.");

        if (await _repo.ExisteDenunciaPendienteAsync(usuarioId, dto.TipoContenido, dto.IdContenido))
            throw AppException.Conflict("Ya tienes una denuncia pendiente sobre este contenido.");

        var ahora = DateTime.Now;
        var denuncia = new Denuncias
        {
            UsuarioId = usuarioId,
            TipoContenido = dto.TipoContenido,
            IdContenido = dto.IdContenido,
            Motivo = dto.Motivo,
            Descripcion = descripcion,
            Estado = (byte)EstadoDenuncia.Recibida,
            Fecha = ahora,
            Activo = true
        };

        await _repo.InsertarAsync(denuncia);
        return denuncia.DenunciaId;
    }
}
