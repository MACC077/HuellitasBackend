using Huellitas.Application.DTOs.Mascotas;

namespace Huellitas.Application.DTOs.Refugios;

// Para HU-REF-01 (Admin)
public class VerificarRefugioDTO
{
    public bool Aprobado { get; set; }
    public string? MotivoRechazo { get; set; }
}

public class SuspenderRefugioDTO
{
    public string Motivo { get; set; } = string.Empty;
}

// Para HU-REF-02 (Refugio)
public class PerfilRefugioDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public Dictionary<string, string>? RedesSociales { get; set; }
    //public string RedesSociales { get; set; } = string.Empty;
    public byte EstadoVerificacion { get; set; }
    public List<RequisitoDTO> Requisitos { get; set; } = new();
}

public class ActualizarRefugioDTO
{
    public string Descripcion { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public Dictionary<string, string>? RedesSociales { get; set; }
    //public string RedesSociales { get; set; } = string.Empty;
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
    public Dictionary<string, string>? RedesSociales { get; set; }
    public bool Verificado { get; set; }
    public List<RequisitoDTO> Requisitos { get; set; } = new();
    public List<MascotaResumenDTO> MascotasDisponibles { get; set; } = new();
}

public class CrearRefugioDTO
{
    public string Nombre { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Encargado { get; set; }
    public string? Descripcion { get; set; }
    public Dictionary<string, string>? RedesSociales { get; set; }

    /// <summary>URLs de soportes en PDF o imagen (jpg, jpeg, png, webp).</summary>
    public List<string> SoportesUrls { get; set; } = new();
}

public class RefugioSoporteDTO
{
    public int SoporteId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Archivo { get; set; } = string.Empty;
    public DateTime FechaCarga { get; set; }
}

public class RefugioAdminDTO
{
    public int RefugioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Nit { get; set; } = string.Empty;
    public string Direccion { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Encargado { get; set; }
    public string? Descripcion { get; set; }
    public Dictionary<string, string>? RedesSociales { get; set; }
    public string EstadoVerificacion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaVerificacion { get; set; }
    public string? MotivoRechazo { get; set; }
    public List<RefugioSoporteDTO> Soportes { get; set; } = new();
}
