using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Apadrinamientos
{
    public int ApadrinamientoId { get; set; }

    public int UsuarioId { get; set; }

    public int MascotaId { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFin { get; set; }

    public byte Estado { get; set; }

    public string? Observacion { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Mascotas Mascota { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;
}
