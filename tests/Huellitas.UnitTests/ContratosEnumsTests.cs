using Huellitas.Infrastructure.Enums;

namespace Huellitas.UnitTests;

/// <summary>
/// Protege el contrato numérico de los enums frente a las columnas TINYINT de SQL Server.
/// Si cambia un valor, la API y la base de datos quedarían desalineadas.
/// </summary>
public class ContratosEnumsTests
{
    [Fact]
    public void EstadoMascota_CoincideConLaBaseDeDatos()
    {
        Assert.Equal(0, (byte)EstadoMascota.Disponible);
        Assert.Equal(1, (byte)EstadoMascota.EnProceso);
        Assert.Equal(2, (byte)EstadoMascota.Adoptada);
        Assert.Equal(3, (byte)EstadoMascota.NoDisponible);
    }

    [Fact]
    public void EstadoSolicitudAdopcion_CoincideConLaBaseDeDatos()
    {
        Assert.Equal(0, (byte)EstadoSolicitudAdopcion.Enviada);
        Assert.Equal(1, (byte)EstadoSolicitudAdopcion.EnRevision);
        Assert.Equal(2, (byte)EstadoSolicitudAdopcion.Entrevista);
        Assert.Equal(3, (byte)EstadoSolicitudAdopcion.Aprobada);
        Assert.Equal(4, (byte)EstadoSolicitudAdopcion.Rechazada);
        Assert.Equal(5, (byte)EstadoSolicitudAdopcion.Finalizada);
        Assert.Equal(6, (byte)EstadoSolicitudAdopcion.Cancelada);
        Assert.Equal(7, (byte)EstadoSolicitudAdopcion.Cerrada);
    }

    [Fact]
    public void EstadoUsuario_CoincideConLaBaseDeDatos()
    {
        Assert.Equal(0, (byte)EstadoUsuario.PendienteVerificacion);
        Assert.Equal(1, (byte)EstadoUsuario.Activo);
        Assert.Equal(2, (byte)EstadoUsuario.Bloqueado);
        Assert.Equal(3, (byte)EstadoUsuario.Desactivado);
    }

    [Fact]
    public void EstadoVerificacionRefugio_CoincideConLaBaseDeDatos()
    {
        Assert.Equal(0, (byte)EstadoVerificacionRefugio.Pendiente);
        Assert.Equal(1, (byte)EstadoVerificacionRefugio.Verificado);
        Assert.Equal(2, (byte)EstadoVerificacionRefugio.Rechazado);
        Assert.Equal(3, (byte)EstadoVerificacionRefugio.Suspendido);
    }

    [Fact]
    public void EstadoVisitaEntrevista_CoincideConLaBaseDeDatos()
    {
        Assert.Equal(0, (byte)EstadoVisitaEntrevista.Propuesta);
        Assert.Equal(1, (byte)EstadoVisitaEntrevista.Confirmada);
        Assert.Equal(2, (byte)EstadoVisitaEntrevista.Reprogramada);
        Assert.Equal(3, (byte)EstadoVisitaEntrevista.Realizada);
    }
}
