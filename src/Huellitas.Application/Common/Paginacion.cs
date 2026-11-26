using Huellitas.Application.Common.Exceptions;

namespace Huellitas.Application.Common;

/// <summary>
/// Reglas comunes de paginación para evitar Skip/Take con valores inválidos.
/// </summary>
public static class Paginacion
{
    public const int TamanoMaximo = 100;

    public static (int Pagina, int Tamano) Validar(int pagina, int tamano, int tamanoMaximo = TamanoMaximo)
    {
        if (pagina < 1)
            throw AppException.BadRequest("La página debe ser mayor o igual a 1.");

        if (tamano < 1 || tamano > tamanoMaximo)
            throw AppException.BadRequest($"El tamaño de página debe estar entre 1 y {tamanoMaximo}.");

        return (pagina, tamano);
    }
}
