using Huellitas.Application.Common;

namespace Huellitas.UnitTests;

public class FirmasArchivoTests
{
    private static MemoryStream Flujo(params byte[] bytes) => new(bytes);

    [Fact]
    public async Task AceptaJpeg()
    {
        var valido = await FirmasArchivo.CoincideAsync(Flujo(0xFF, 0xD8, 0xFF, 0xE0), ".jpg");
        Assert.True(valido);
    }

    [Fact]
    public async Task AceptaPng()
    {
        var valido = await FirmasArchivo.CoincideAsync(
            Flujo(0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00), ".png");
        Assert.True(valido);
    }

    [Fact]
    public async Task AceptaWebp()
    {
        var valido = await FirmasArchivo.CoincideAsync(
            Flujo((byte)'R', (byte)'I', (byte)'F', (byte)'F', 0, 0, 0, 0,
                  (byte)'W', (byte)'E', (byte)'B', (byte)'P'), ".webp");
        Assert.True(valido);
    }

    [Fact]
    public async Task AceptaPdf()
    {
        var valido = await FirmasArchivo.CoincideAsync(
            Flujo((byte)'%', (byte)'P', (byte)'D', (byte)'F', (byte)'-'), ".pdf");
        Assert.True(valido);
    }

    [Fact]
    public async Task RechazaContenidoFalso()
    {
        var valido = await FirmasArchivo.CoincideAsync(
            Flujo((byte)'<', (byte)'h', (byte)'t', (byte)'m', (byte)'l'), ".jpg");
        Assert.False(valido);
    }

    [Fact]
    public async Task RestauraLaPosicionDelStream()
    {
        var flujo = Flujo(0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x00);

        await FirmasArchivo.CoincideAsync(flujo, ".jpg");

        Assert.Equal(0, flujo.Position);
    }
}
