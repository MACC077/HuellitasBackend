using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Refugios
{
    public int RefugioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Nit { get; set; } = null!;

    public string Direccion { get; set; } = null!;

    public string? Telefono { get; set; }

    public string? Correo { get; set; }

    public string? Encargado { get; set; }

    public string? RedesSociales { get; set; }

    public string? Descripcion { get; set; }

    public byte EstadoVerificacion { get; set; }

    public DateTime? FechaVerificacion { get; set; }

    public int? VerificadoPorId { get; set; }

    public string? MotivoRechazo { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<Calificaciones> Calificaciones { get; set; } = new List<Calificaciones>();

    public virtual ICollection<Mascotas> Mascota { get; set; } = new List<Mascotas>();

    public virtual ICollection<Necesidades> Necesidades { get; set; } = new List<Necesidades>();

    public virtual ICollection<ReportesCiudadano> ReportesCiudadanos { get; set; } = new List<ReportesCiudadano>();

    public virtual ICollection<RequisitosAdopcion> RequisitosAdopcions { get; set; } = new List<RequisitosAdopcion>();

    public virtual ICollection<Usuarios> Usuarios { get; set; } = new List<Usuarios>();

    public virtual Usuarios? VerificadoPor { get; set; }
}
