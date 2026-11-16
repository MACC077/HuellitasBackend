namespace Huellitas.Application.Common;

/// <summary>Página de resultados con metadatos de paginación.</summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalRegistros { get; set; }

    /// <summary>Total de páginas; 0 si no hay registros o el tamaño es inválido.</summary>
    public int TotalPaginas => TamanoPagina <= 0 || TotalRegistros <= 0
        ? 0
        : (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);
}
