using System.Security.Claims;

namespace Huellitas.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Extrae el ID del usuario desde el claim NameIdentifier del JWT.
    /// </summary>
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var claim = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var id))
            return 0; // Si es 0, Application lanzará el error 401/NotFound

        return id;
    }
}