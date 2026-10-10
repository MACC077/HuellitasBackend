using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class RegistrosClinico
{
    public int RegistroClinicoId { get; set; }

    public int MascotaId { get; set; }

    public byte Tipo { get; set; }

    public DateOnly Fecha { get; set; }

    public string? Descripcion { get; set; }

    public string? SoporteVeterinario { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Mascotas Mascota { get; set; } = null!;
}
