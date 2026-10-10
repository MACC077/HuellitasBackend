using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Razas
{
    public int RazaId { get; set; }

    public int EspecieId { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual Especies Especie { get; set; } = null!;

    public virtual ICollection<Mascotas> Mascota { get; set; } = new List<Mascotas>();
}
