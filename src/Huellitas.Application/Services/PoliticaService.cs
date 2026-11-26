using System.Transactions;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Politicas;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Services;

public class PoliticaService : IPoliticaService
{
    private readonly IUsuarioRepository _repo;
    private readonly ITrazabilidadRepository _trazabilidad;

    public PoliticaService(IUsuarioRepository repo, ITrazabilidadRepository trazabilidad)
    {
        _repo = repo;
        _trazabilidad = trazabilidad;
    }

    public async Task<PoliticaDTO> ObtenerActivaAsync()
    {
        var politica = await _repo.ObtenerPoliticaActivaAsync()
            ?? throw AppException.NotFound("No hay una política de datos activa.");

        return ToDto(politica);
    }

    public async Task<IEnumerable<PoliticaDTO>> ObtenerTodasAsync()
    {
        var politicas = await _repo.ObtenerPoliticasAsync();
        return politicas.Select(ToDto).ToList();
    }

    public async Task<int> CrearAsync(CrearPoliticaDTO dto, int adminId)
    {
        var version = (dto.Version ?? string.Empty).Trim();
        var contenido = (dto.Contenido ?? string.Empty).Trim();

        if (version.Length == 0)
            throw AppException.BadRequest("La versión de la política es obligatoria.");

        if (version.Length > 40)
            throw AppException.BadRequest("La versión no puede superar los 40 caracteres.");

        if (contenido.Length == 0)
            throw AppException.BadRequest("El contenido de la política es obligatorio.");

        var fecha = dto.FechaPublicacion ?? DateTime.Now;

        using var scope = CrearTransaccion();

        // Solo puede existir una política activa: al publicar una nueva se desactivan las anteriores.
        await _repo.DesactivarPoliticasActivasAsync();

        var politica = new PoliticasDato
        {
            Version = version,
            Contenido = contenido,
            FechaPublicacion = fecha,
            Activo = true
        };

        await _repo.InsertarPoliticaAsync(politica);

        await _trazabilidad.InsertarAuditoriaAsync(new RegistrosAuditorias
        {
            UsuarioId = adminId > 0 ? adminId : null,
            Accion = "CrearPolitica",
            EntidadAfectada = "PoliticasDatos",
            IdEntidad = politica.PoliticaId,
            ValorAnterior = null,
            ValorNuevo = $"Version={version}; FechaPublicacion={fecha:O}",
            FechaCreacion = DateTime.Now
        });

        scope.Complete();
        return politica.PoliticaId;
    }

    public async Task AceptarAsync(int usuarioId, AceptarPoliticaDTO dto)
    {
        if (usuarioId <= 0)
            throw AppException.Unauthorized("Debes iniciar sesión para aceptar la política.");

        var activa = await _repo.ObtenerPoliticaActivaAsync()
            ?? throw AppException.NotFound("No hay una política de datos activa.");

        if (dto.PoliticaId.HasValue && dto.PoliticaId.Value != activa.PoliticaId)
            throw AppException.Conflict("La política cambió. Recarga la información e inténtalo de nuevo.");

        if (!string.IsNullOrWhiteSpace(dto.Version) &&
            !string.Equals(dto.Version.Trim(), activa.Version, StringComparison.Ordinal))
        {
            throw AppException.Conflict("La versión de la política cambió. Recarga la información e inténtalo de nuevo.");
        }

        if (await _repo.HaAceptadoPoliticaAsync(usuarioId, activa.PoliticaId))
            return;

        using var scope = CrearTransaccion();

        // Se conserva el historial, pero solo el consentimiento de la política vigente queda activo.
        await _repo.DesactivarConsentimientosActivosAsync(usuarioId);

        await _repo.InsertarConsentimientoAsync(new ConsentimientosDatos
        {
            UsuarioId = usuarioId,
            PoliticaId = activa.PoliticaId,
            VersionAceptada = activa.Version,
            FechaAceptacion = DateTime.Now,
            Activo = true
        });

        scope.Complete();
    }

    private static PoliticaDTO ToDto(PoliticasDato politica)
    {
        return new PoliticaDTO
        {
            PoliticaId = politica.PoliticaId,
            Version = politica.Version,
            Contenido = politica.Contenido,
            FechaPublicacion = politica.FechaPublicacion,
            Activo = politica.Activo
        };
    }

    private static TransactionScope CrearTransaccion()
    {
        return new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted },
            TransactionScopeAsyncFlowOption.Enabled);
    }
}
