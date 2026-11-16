using Huellitas.Application.Common;

namespace Huellitas.UnitTests;

public class PaginacionTests
{
    [Theory]
    [InlineData(1, 10, 1, 10)]
    [InlineData(3, 20, 3, 20)]
    [InlineData(0, 10, 1, 10)]
    [InlineData(-1, 10, 1, 10)]
    [InlineData(1, 0, 1, Paginacion.TamanoPorDefecto)]
    [InlineData(1, -5, 1, Paginacion.TamanoPorDefecto)]
    [InlineData(2, 500, 2, Paginacion.TamanoMaximo)]
    public void Normalizar_AcotaPaginaYTamano(int pagina, int tamano, int paginaEsperada, int tamanoEsperado)
    {
        var (paginaNormalizada, tamanoNormalizado) = Paginacion.Normalizar(pagina, tamano);

        Assert.Equal(paginaEsperada, paginaNormalizada);
        Assert.Equal(tamanoEsperado, tamanoNormalizado);
    }
}
