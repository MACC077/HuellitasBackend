namespace Huellitas.Application.Common;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Pagina { get; set; }
    public int TamanoPagina { get; set; }
    public int TotalRegistros { get; set; }

    public int TotalPaginas => (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);
}