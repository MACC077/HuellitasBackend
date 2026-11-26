using Huellitas.Application.DTOs.Reportes;
using Huellitas.Application.Mappings;
using Huellitas.Application.Validators;

namespace Huellitas.UnitTests;

public class ReportesEdicionTests
{
    private static ActualizarReporteDTO DtoValido()
    {
        return new ActualizarReporteDTO
        {
            Tipo = 0,
            Descripcion = "Perro en la calle",
            Latitud = 3.45m,
            Longitud = -76.53m,
            FotosUrls = new List<string> { "https://ejemplo.com/foto.jpg" }
        };
    }

    [Fact]
    public void Validator_AceptaDtoValido()
    {
        var result = new ActualizarReporteValidator().Validate(DtoValido());

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validator_RechazaTipoYCoordenadasInvalidas()
    {
        var dto = DtoValido();
        dto.Tipo = 99;
        dto.Latitud = 120m;
        dto.Longitud = -200m;

        var result = new ActualizarReporteValidator().Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.Tipo));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.Latitud));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(dto.Longitud));
    }

    [Fact]
    public void Validator_RechazaUrlQueNoEsHttp()
    {
        var dto = DtoValido();
        dto.FotosUrls = new List<string> { "ftp://ejemplo.com/foto.jpg" };

        var result = new ActualizarReporteValidator().Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.StartsWith(nameof(dto.FotosUrls)));
    }

    [Fact]
    public void Mapping_NormalizaDescripcionYCamposOpcionalesVacios()
    {
        var dto = DtoValido();
        dto.Descripcion = "  Perro en la calle  ";
        dto.Direccion = "   ";
        dto.PuntoReferencia = "  Frente al parque  ";

        var datos = dto.ToDatosActualizados();

        Assert.Equal("Perro en la calle", datos.Descripcion);
        Assert.Null(datos.Ubicacion_Direccion);
        Assert.Equal("Frente al parque", datos.Ubicacion_PuntoReferencia);
        Assert.Equal(dto.Latitud, datos.Ubicacion_Latitud);
        Assert.Equal(dto.Longitud, datos.Ubicacion_Longitud);
    }
}
