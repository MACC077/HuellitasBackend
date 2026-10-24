namespace Huellitas.Application.DTOs.Catalogos;

public class EspecieDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class RazaDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public int EspecieId { get; set; }
}

public class TamanoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class ColorDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class TemperamentoDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }
}

public class TipoViviendaDTO
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}