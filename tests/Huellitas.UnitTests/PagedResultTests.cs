using Huellitas.Application.Common;

namespace Huellitas.UnitTests;

public class PagedResultTests
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(1, 10, 1)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    [InlineData(25, 5, 5)]
    public void TotalPaginas_SeCalculaCorrectamente(int totalRegistros, int tamanoPagina, int esperado)
    {
        var resultado = new PagedResult<int>
        {
            TotalRegistros = totalRegistros,
            TamanoPagina = tamanoPagina
        };

        Assert.Equal(esperado, resultado.TotalPaginas);
    }

    [Fact]
    public void Items_SeInicializaVacio()
    {
        var resultado = new PagedResult<string>();

        Assert.NotNull(resultado.Items);
        Assert.Empty(resultado.Items);
    }
}
