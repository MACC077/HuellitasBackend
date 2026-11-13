using Huellitas.Application.DTOs.Mascotas;

namespace Huellitas.Application.DTOs.Refugios;

// Para HU-REF-01 (Admin)
public class VerificarRefugioDTO
{
    public bool Aprobado { get; set; }
    public string? MotivoRechazo { get; set; }
}

// Para HU-REF-02 (Refugio)
public class PerfilRefugioDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string RedesSociales { get; set; } = string.Empty;
    public byte EstadoVerificacion { get; set; }
    public List<RequisitoDTO> Requisitos { get; set; } = new();
}

public class ActualizarRefugioDTO
{
    public string Descripcion { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string RedesSociales { get; set; } = string.Empty;
}

public class RequisitoDTO
{
    public int? RequisitoId { get; set; } // Null si es nuevo
    public string Descripcion { get; set; } = string.Empty;
    public bool EsObligatorio { get; set; }
    public byte Orden { get; set; }
}


// Para el listado público
public class RefugioResumenDTO
{
    public int RefugioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string? FotoUrl { get; set; } // Si luego agregan foto de portada
}

// Para la ficha pública
public class RefugioPublicoDTO
{
    public int RefugioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? RedesSociales { get; set; }
    public bool Verificado { get; set; }
    public List<RequisitoDTO> Requisitos { get; set; } = new();
    public List<MascotaResumenDTO> MascotasDisponibles { get; set; } = new();
}