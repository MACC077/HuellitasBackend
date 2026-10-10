using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Calificaciones
{
    public int CalificacionId { get; set; }

    public int RefugioId { get; set; }

    public int UsuarioId { get; set; }

    public int AdopcionId { get; set; }

    public byte Puntaje { get; set; }

    public string? Comentario { get; set; }

    public byte EstadoModeracion { get; set; }

    public DateTime Fecha { get; set; }

    public bool Activo { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Adopciones Adopcion { get; set; } = null!;

    public virtual Refugios Refugio { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;
}
