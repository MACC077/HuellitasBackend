namespace Huellitas.Application.Common;

/// <summary>Normaliza los parámetros de paginación de las consultas.</summary>
public static class Paginacion
{
    public const int TamanoPorDefecto = 10;
    public const int TamanoMaximo = 50;

    /// <summary>Devuelve una página mayor o igual a 1 y un tamaño entre 1 y el máximo permitido.</summary>
    public static (int Pagina, int Tamano) Normalizar(int pagina, int tamano)
    {
        var paginaNormalizada = pagina < 1 ? 1 : pagina;
        var tamanoNormalizado = tamano < 1
            ? TamanoPorDefecto
            : Math.Min(tamano, TamanoMaximo);

        return (paginaNormalizada, tamanoNormalizado);
    }
}
