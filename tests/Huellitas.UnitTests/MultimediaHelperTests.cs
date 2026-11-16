using Huellitas.Application.Common;

namespace Huellitas.UnitTests;

public class MultimediaHelperTests
{
    [Theory]
    [InlineData("https://www.youtube.com/watch?v=abc123")]
    [InlineData("https://youtu.be/abc123")]
    [InlineData("https://vimeo.com/123456")]
    [InlineData("https://res.cloudinary.com/demo/video/upload/v1/clip.mp4")]
    [InlineData("https://cdn.huellitas.com/videos/presentacion.MOV")]
    public void EsVideo_ReconoceEnlacesDeVideo(string url)
    {
        Assert.True(MultimediaHelper.EsVideo(url));
    }

    [Theory]
    [InlineData("https://res.cloudinary.com/demo/image/upload/v1/foto.jpg")]
    [InlineData("https://res.cloudinary.com/demo/image/upload/v1/foto.webp")]
    [InlineData("")]
    [InlineData(null)]
    public void EsVideo_DescartaImagenesYValoresVacios(string? url)
    {
        Assert.False(MultimediaHelper.EsVideo(url));
    }
}
