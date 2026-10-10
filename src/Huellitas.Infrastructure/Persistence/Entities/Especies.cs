using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Especies
{
    public int EspecieId { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<Mascotas> Mascota { get; set; } = new List<Mascotas>();

    public virtual ICollection<Razas> Razas { get; set; } = new List<Razas>();
}
