using System.Text.RegularExpressions;

namespace Huellitas.Application.Common;

/// <summary>Clasifica y separa los enlaces de multimedia de una mascota.</summary>
public static class MultimediaHelper
{
    public const byte TipoFoto = 0;
    public const byte TipoVideo = 1;

    private static readonly Regex PatronVideo = new(
        @"(youtube\.com|youtu\.be|vimeo\.com|dailymotion\.com|tiktok\.com|\.(mp4|mov|webm|avi|mkv)(\?|#|$))",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>Indica si la URL corresponde a un video y no a una imagen.</summary>
    public static bool EsVideo(string? url) =>
        !string.IsNullOrWhiteSpace(url) && PatronVideo.IsMatch(url);
}
