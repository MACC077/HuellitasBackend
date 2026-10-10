using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Bitacoras
{
    public long BitacoraId { get; set; }

    public int MascotaId { get; set; }

    public byte Tipo { get; set; }

    public string Descripcion { get; set; } = null!;

    public DateTime FechaCreacion { get; set; }

    public virtual Mascotas Mascota { get; set; } = null!;
}
