using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Temperamentos
{
    public int TemperamentoId { get; set; }

    public string Nombre { get; set; } = null!;

    public bool Activo { get; set; }

    public virtual ICollection<Mascotas> Mascota { get; set; } = new List<Mascotas>();
}
