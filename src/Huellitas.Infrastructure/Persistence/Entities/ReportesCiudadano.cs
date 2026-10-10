using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class ReportesCiudadano
{
    public int ReporteId { get; set; }

    public int UsuarioId { get; set; }

    public int? RefugioId { get; set; }

    public int? MascotaId { get; set; }

    public string NumeroReporte { get; set; } = null!;

    public byte Tipo { get; set; }

    public string Descripcion { get; set; } = null!;

    public DateTime FechaReporte { get; set; }

    public byte Estado { get; set; }

    public string? MotivoCierre { get; set; }

    public decimal Ubicacion_Latitud { get; set; }

    public decimal Ubicacion_Longitud { get; set; }

    public string? Ubicacion_Direccion { get; set; }

    public string? Ubicacion_PuntoReferencia { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Mascotas? Mascota { get; set; }

    public virtual ICollection<Multimedia> Multimedia { get; set; } = new List<Multimedia>();

    public virtual Refugios? Refugio { get; set; }

    public virtual Usuarios Usuario { get; set; } = null!;
}
