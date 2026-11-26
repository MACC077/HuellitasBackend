using Huellitas.Application.Common.Exceptions;

namespace Huellitas.UnitTests;

public class AppExceptionTests
{
    [Fact]
    public void BadRequest_DevuelveEstado400()
    {
        var ex = AppException.BadRequest("solicitud inválida");

        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("solicitud inválida", ex.Message);
    }

    [Fact]
    public void NotFound_DevuelveEstado404()
    {
        Assert.Equal(404, AppException.NotFound("no existe").StatusCode);
    }

    [Fact]
    public void Conflict_DevuelveEstado409()
    {
        Assert.Equal(409, AppException.Conflict("conflicto").StatusCode);
    }

    [Fact]
    public void Unauthorized_DevuelveEstado401()
    {
        Assert.Equal(401, AppException.Unauthorized("no autorizado").StatusCode);
    }

    [Fact]
    public void Forbidden_DevuelveEstado403()
    {
        Assert.Equal(403, AppException.Forbidden("prohibido").StatusCode);
    }

    [Fact]
    public void Internal_DevuelveEstado500()
    {
        Assert.Equal(500, AppException.Internal("error").StatusCode);
    }
}
