using Huellitas.Application.Common;
using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.Services;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.UnitTests;

public class TrazabilidadTests
{
    [Fact]
    public void Paginacion_RechazaPaginaInvalida()
    {
        Assert.Throws<AppException>(() => Paginacion.Validar(0, 10));
        Assert.Throws<AppException>(() => Paginacion.Validar(-1, 10));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(101)]
    public void Paginacion_RechazaTamanoInvalido(int tamano)
    {
        Assert.Throws<AppException>(() => Paginacion.Validar(1, tamano));
    }

    [Fact]
    public void Paginacion_AceptaValoresValidos()
    {
        var (pagina, tamano) = Paginacion.Validar(3, 100);

        Assert.Equal(3, pagina);
        Assert.Equal(100, tamano);
    }

    [Fact]
    public void EnumsNuevos_CoincidenConLaBaseDeDatos()
    {
        Assert.Equal(0, (byte)CanalNotificacion.InApp);
        Assert.Equal(1, (byte)CanalNotificacion.Push);
        Assert.Equal(2, (byte)CanalNotificacion.Correo);

        Assert.Equal(0, (byte)EstadoSeguimiento.Pendiente);
        Assert.Equal(1, (byte)EstadoSeguimiento.Enviado);
        Assert.Equal(2, (byte)EstadoSeguimiento.Vencido);

        Assert.Equal(0, (byte)EntidadAfectada.SolicitudAdopcion);
        Assert.Equal(1, (byte)EntidadAfectada.ReporteCiudadano);
    }

    [Fact]
    public async Task MarcarLeida_NoPermiteNotificacionAjena()
    {
        var repo = new NotificacionRepositoryFake
        {
            Notificacion = new Notificaciones
            {
                NotificacionId = 10,
                UsuarioId = 1,
                Tipo = "Sistema",
                Mensaje = "Mensaje",
                FechaEnvio = DateTime.Now
            }
        };
        var service = new NotificacionService(repo);

        var ex = await Assert.ThrowsAsync<AppException>(() => service.MarcarLeidaAsync(10, 2));

        Assert.Equal(404, ex.StatusCode);
        Assert.Equal(2, repo.UltimoUsuarioConsultado);
        Assert.False(repo.Actualizada);
    }

    [Fact]
    public async Task MarcarLeida_MarcaNotificacionPropia()
    {
        var repo = new NotificacionRepositoryFake
        {
            Notificacion = new Notificaciones
            {
                NotificacionId = 10,
                UsuarioId = 1,
                Tipo = "Sistema",
                Mensaje = "Mensaje",
                FechaEnvio = DateTime.Now
            }
        };
        var service = new NotificacionService(repo);

        await service.MarcarLeidaAsync(10, 1);

        Assert.True(repo.Notificacion!.Leida);
        Assert.True(repo.Actualizada);
    }

    private class NotificacionRepositoryFake : INotificacionRepository
    {
        public Notificaciones? Notificacion { get; set; }
        public int? UltimoUsuarioConsultado { get; private set; }
        public bool Actualizada { get; private set; }

        public Task InsertarAsync(Notificaciones notificacion)
        {
            Notificacion = notificacion;
            return Task.CompletedTask;
        }

        public Task<(IEnumerable<Notificaciones> Items, int Total)> ObtenerPorUsuarioAsync(int usuarioId, int pagina, int tamano)
            => Task.FromResult<(IEnumerable<Notificaciones>, int)>((Array.Empty<Notificaciones>(), 0));

        public Task<Notificaciones?> ObtenerPorIdAsync(long notificacionId, int usuarioId)
        {
            UltimoUsuarioConsultado = usuarioId;
            var notificacion = Notificacion;

            return Task.FromResult(
                notificacion != null && notificacion.NotificacionId == notificacionId && notificacion.UsuarioId == usuarioId
                    ? notificacion
                    : null);
        }

        public Task ActualizarAsync(Notificaciones notificacion)
        {
            Actualizada = true;
            Notificacion = notificacion;
            return Task.CompletedTask;
        }
    }
}
