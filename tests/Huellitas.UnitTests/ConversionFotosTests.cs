using Huellitas.Application.Common;
using Huellitas.Application.DTOs.Mascotas;
using Huellitas.Application.DTOs.Reportes;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Services;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.UnitTests;

/// <summary>
/// Verifica el punto 7 del criterio del front: la conversión reporte → mascota
/// hace join con ReporteMultimedia y copia las fotos activas a la ficha nueva.
/// </summary>
public class ConversionFotosTests
{
    [Fact]
    public async Task ConvertirAFichaMascota_CopiaSoloFotosActivasDelReporte()
    {
        var reporte = new ReportesCiudadano
        {
            ReporteId = 10,
            RefugioId = 5,
            Estado = (byte)EstadoReporte.Rescatado,
            Activo = true,
            Descripcion = "Perro rescatado",
            Fotos = new List<ReporteMultimedia>
            {
                new() { Id = 1, RutaArchivo = "https://cdn.test/uno.jpg", Activo = true },
                new() { Id = 2, RutaArchivo = "https://cdn.test/dos.jpg", Activo = true },
                new() { Id = 3, RutaArchivo = "https://cdn.test/borrada.jpg", Activo = false }
            }
        };

        var reporteRepo = new FakeReporteRepository(reporte);
        var mascotaService = new CapturandoMascotaService();
        var service = new ReporteService(
            reporteRepo,
            new FakeRefugioRepository(),
            mascotaService,
            new FakeCatalogoRepository());

        var dto = new ConvertirMascotaDTO
        {
            Nombre = "Lucky",
            EspecieId = 1,
            TamanoId = 1,
            ColorId = 1,
            Sexo = 1
        };

        var mascotaId = await service.ConvertirAFichaMascotaAsync(10, dto, 100);

        Assert.Equal(99, mascotaId);
        Assert.True(reporteRepo.Vinculado);
        Assert.NotNull(mascotaService.Capturado);
        Assert.Equal(2, mascotaService.Capturado.FotosUrls.Count);
        Assert.Contains("https://cdn.test/uno.jpg", mascotaService.Capturado.FotosUrls);
        Assert.Contains("https://cdn.test/dos.jpg", mascotaService.Capturado.FotosUrls);
        Assert.DoesNotContain("https://cdn.test/borrada.jpg", mascotaService.Capturado.FotosUrls);
        Assert.Equal((byte)2, mascotaService.Capturado.Procedencia);
    }

    private sealed class CapturandoMascotaService : IMascotaService
    {
        public CrearMascotaDTO? Capturado { get; private set; }

        public Task<int> RegistrarAsync(CrearMascotaDTO dto, int usuarioId)
        {
            Capturado = dto;
            return Task.FromResult(99);
        }

        public Task ActualizarAsync(int mascotaId, ActualizarMascotaDTO dto, int usuarioId) => Task.CompletedTask;
        public Task CambiarEstadoAsync(int mascotaId, CambiarEstadoMascotaDTO dto, int usuarioId) => Task.CompletedTask;
        public Task DarDeBajaAsync(int mascotaId, string motivo, int usuarioId) => Task.CompletedTask;
        public Task<PagedResult<MascotaResumenDTO>> ObtenerMisMascotasAsync(int usuarioId, int pagina, int tamano)
            => Task.FromResult(new PagedResult<MascotaResumenDTO>());
        public Task<PagedResult<MascotaResumenDTO>> ObtenerCatalogoAsync(FiltroMascotaDTO filtro)
            => Task.FromResult(new PagedResult<MascotaResumenDTO>());
        public Task<MascotaDetalleDTO> ObtenerDetalleAsync(int mascotaId, int usuarioId, bool esAdmin)
            => throw new NotImplementedException();
        public Task AgregarRegistroClinicoAsync(int mascotaId, CrearRegistroClinicoDTO dto, int usuarioId) => Task.CompletedTask;
        public Task<PagedResult<RegistroClinicoDTO>> ObtenerHistorialClinicoAsync(int mascotaId, int pagina, int tamano)
            => Task.FromResult(new PagedResult<RegistroClinicoDTO>());
        public Task<CompartirMascotaDTO> ObtenerDatosParaCompartirAsync(int mascotaId, string baseUrlFrontend)
            => throw new NotImplementedException();
    }

    private sealed class FakeReporteRepository : IReporteRepository
    {
        private readonly ReportesCiudadano _reporte;

        public FakeReporteRepository(ReportesCiudadano reporte) => _reporte = reporte;

        public bool Vinculado { get; private set; }

        public Task<ReportesCiudadano?> ObtenerParaConversionAsync(int reporteId)
            => Task.FromResult<ReportesCiudadano?>(_reporte);

        public Task<bool> VincularMascotaAsync(int reporteId, int refugioId, int mascotaId, CambiosDeEstado cambio)
        {
            Vinculado = true;
            _reporte.MascotaId = mascotaId;
            return Task.FromResult(true);
        }

        public Task<ReportesCiudadano> InsertarAsync(ReportesCiudadano reporte, IEnumerable<ReporteMultimedia> fotos, CambiosDeEstado? cambioInicial)
            => throw new NotImplementedException();
        public Task<ReportesCiudadano?> ObtenerDetalleAsync(int reporteId) => throw new NotImplementedException();
        public Task<ReportesCiudadano?> ObtenerBasicoAsync(int reporteId) => Task.FromResult<ReportesCiudadano?>(_reporte);
        public Task<(IEnumerable<ReportesCiudadano> Items, int Total)> ObtenerBandejaAsync(int refugioId, int pagina, int tamano)
            => throw new NotImplementedException();
        public Task<IEnumerable<ReportesCiudadano>> ObtenerMapaAsync() => throw new NotImplementedException();
        public Task<bool> TomarAsync(int reporteId, int? refugioId, CambiosDeEstado cambio) => throw new NotImplementedException();
        public Task<bool> CambiarEstadoAsync(int reporteId, int? refugioId, byte estadoAnterior, byte estadoNuevo, string? motivoCierre, CambiosDeEstado cambio)
            => throw new NotImplementedException();
        public Task<bool> EliminarAsync(int reporteId, byte? estadoEsperado, bool exigirSinRefugio, CambiosDeEstado cambio)
            => throw new NotImplementedException();
        public Task<bool> ActualizarAsync(int reporteId, ReportesCiudadano cambios, IEnumerable<ReporteMultimedia>? fotos, CambiosDeEstado cambio)
            => throw new NotImplementedException();
    }

    private sealed class FakeRefugioRepository : IRefugioRepository
    {
        public Task<Refugios?> ObtenerPorUsuarioIdAsync(int usuarioId)
        {
            return Task.FromResult<Refugios?>(new Refugios
            {
                RefugioId = 5,
                Nombre = "Refugio",
                Nit = "123",
                Direccion = "Calle 1",
                EstadoVerificacion = (byte)EstadoVerificacionRefugio.Verificado,
                Activo = true,
                FechaCreacion = DateTime.Now
            });
        }

        public Task<Refugios?> ObtenerPorIdAsync(int refugioId) => throw new NotImplementedException();
        public Task<Refugios> InsertarAsync(Refugios refugio) => throw new NotImplementedException();
        public Task ActualizarAsync(Refugios refugio) => Task.CompletedTask;
        public Task<IEnumerable<RequisitosAdopcion>> ObtenerRequisitosPorRefugioAsync(int refugioId) => throw new NotImplementedException();
        public Task InsertarRequisitoAsync(RequisitosAdopcion requisito) => throw new NotImplementedException();
        public Task EliminarRequisitoAsync(int requisitoId) => throw new NotImplementedException();
        public Task<(IEnumerable<Refugios> Items, int Total)> ObtenerRefugiosPublicosAsync(int pagina, int tamano) => throw new NotImplementedException();
        public Task<(IEnumerable<Refugios> Items, int Total)> ObtenerRefugiosPorEstadoAsync(byte estado, int pagina, int tamano) => throw new NotImplementedException();
        public Task<Refugios?> ObtenerPerfilPublicoPorIdAsync(int refugioId) => throw new NotImplementedException();
        public Task<bool> ExisteNitAsync(string nit) => throw new NotImplementedException();
        public Task InsertarSoportesAsync(IEnumerable<RefugioSoportes> soportes) => throw new NotImplementedException();
        public Task<IEnumerable<RefugioSoportes>> ObtenerSoportesAsync(int refugioId) => throw new NotImplementedException();
    }

    private sealed class FakeCatalogoRepository : ICatalogoRepository
    {
        public Task<IEnumerable<Especies>> ObtenerEspeciesAsync()
            => Task.FromResult<IEnumerable<Especies>>(new[] { new Especies { EspecieId = 1, Nombre = "Perro" } });
        public Task<IEnumerable<Razas>> ObtenerRazasPorEspecieAsync(int especieId)
            => Task.FromResult<IEnumerable<Razas>>(Array.Empty<Razas>());
        public Task<IEnumerable<Tamanos>> ObtenerTamanosAsync()
            => Task.FromResult<IEnumerable<Tamanos>>(new[] { new Tamanos { TamanoId = 1, Nombre = "Mediano" } });
        public Task<IEnumerable<Colores>> ObtenerColoresAsync()
            => Task.FromResult<IEnumerable<Colores>>(new[] { new Colores { ColorId = 1, Nombre = "Café" } });
        public Task<IEnumerable<Temperamentos>> ObtenerTemperamentosAsync()
            => Task.FromResult<IEnumerable<Temperamentos>>(Array.Empty<Temperamentos>());
        public Task<IEnumerable<TiposVivienda>> ObtenerTiposViviendaAsync()
            => Task.FromResult<IEnumerable<TiposVivienda>>(Array.Empty<TiposVivienda>());
    }
}
