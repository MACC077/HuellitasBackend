using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class SolicitudesAdopcion
{
    public int SolicitudId { get; set; }

    public int UsuarioId { get; set; }

    public int MascotaId { get; set; }

    public string Codigo { get; set; } = null!;

    public DateTime FechaEnvio { get; set; }

    public byte Estado { get; set; }

    public string? RespuestasFormulario { get; set; }

    public string? Observaciones { get; set; }

    public string? MotivoRechazo { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<Adopciones> Adopciones { get; set; } = new List<Adopciones>();

    public virtual ICollection<DocumentosSoporte> DocumentosSoportes { get; set; } = new List<DocumentosSoporte>();

    public virtual Mascotas Mascota { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;

    public virtual ICollection<VisitasEntrevista> VisitasEntrevista { get; set; } = new List<VisitasEntrevista>();
}
