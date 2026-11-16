namespace Huellitas.Application.Common;

/// <summary>Valida que el contenido binario coincida con la extensión declarada.</summary>
public static class FirmasArchivo
{
    private static readonly Dictionary<string, byte[]> Firmas = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        [".jpeg"] = new byte[] { 0xFF, 0xD8, 0xFF },
        [".png"] = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A },
        [".pdf"] = new byte[] { 0x25, 0x50, 0x44, 0x46 }
    };

    /// <summary>Comprueba la firma del archivo y deja el stream al inicio.</summary>
    public static async Task<bool> CoincideAsync(Stream contenido, string extension)
    {
        var encabezado = new byte[12];
        var leidos = 0;

        while (leidos < encabezado.Length)
        {
            var cantidad = await contenido.ReadAsync(encabezado.AsMemory(leidos, encabezado.Length - leidos));
            if (cantidad == 0)
                break;
            leidos += cantidad;
        }

        if (contenido.CanSeek)
            contenido.Position = 0;

        // WEBP es un contenedor RIFF: la firma "WEBP" está en el byte 8.
        if (extension.Equals(".webp", StringComparison.OrdinalIgnoreCase))
            return leidos >= 12
                && encabezado[0] == (byte)'R' && encabezado[1] == (byte)'I'
                && encabezado[2] == (byte)'F' && encabezado[3] == (byte)'F'
                && encabezado[8] == (byte)'W' && encabezado[9] == (byte)'E'
                && encabezado[10] == (byte)'B' && encabezado[11] == (byte)'P';

        return Firmas.TryGetValue(extension, out var firma)
            && leidos >= firma.Length
            && encabezado.Take(firma.Length).SequenceEqual(firma);
    }
}
