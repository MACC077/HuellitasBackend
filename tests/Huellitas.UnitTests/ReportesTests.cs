using Huellitas.Infrastructure.Enums;

namespace Huellitas.UnitTests;

public class ReportesTests
{
    [Fact]
    public void TipoReporte_CoincideConLaBaseDeDatos()
    {
        Assert.Equal(0, (byte)TipoReporte.EnCalle);
        Assert.Equal(1, (byte)TipoReporte.Herido);
        Assert.Equal(2, (byte)TipoReporte.Maltrato);
        Assert.Equal(3, (byte)TipoReporte.Otro);
    }

    [Fact]
    public void EstadoReporte_CoincideConLaBaseDeDatos()
    {
        Assert.Equal(0, (byte)EstadoReporte.Recibido);
        Assert.Equal(1, (byte)EstadoReporte.EnAtencion);
        Assert.Equal(2, (byte)EstadoReporte.Rescatado);
        Assert.Equal(3, (byte)EstadoReporte.NoEncontrado);
        Assert.Equal(4, (byte)EstadoReporte.Cerrado);
    }
}
