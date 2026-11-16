namespace Huellitas.Application.Common;

/// <summary>Clasifica y separa los enlaces de multimedia de una mascota.</summary>
public static class MultimediaHelper
{
    public const byte TipoFoto = 0;
    public const byte TipoVideo = 1;

    private static readonly string[] ExtensionesVideo =
        { ".mp4", ".mov", ".webm", ".avi", ".mkv" };

    private static readonly string[] DominiosVideo =
        { "youtube.com", "youtu.be", "vimeo.com", "dailymotion.com", "tiktok.com" };

    /// <summary>Indica si la URL corresponde a un video y no a una imagen.</summary>
    public static bool EsVideo(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        var valor = url.Trim();

        if (!Uri.TryCreate(valor, UriKind.Absolute, out var uri))
            return ExtensionesVideo.Any(e => valor.EndsWith(e, StringComparison.OrdinalIgnoreCase));

        var host = uri.Host.ToLowerInvariant();
        if (DominiosVideo.Any(d => host == d || host.EndsWith("." + d)))
            return true;

        return ExtensionesVideo.Any(e => uri.AbsolutePath.EndsWith(e, StringComparison.OrdinalIgnoreCase));
    }
}
