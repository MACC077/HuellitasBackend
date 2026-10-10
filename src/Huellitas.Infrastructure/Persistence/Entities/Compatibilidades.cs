using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Compatibilidades
{
    public int CompatibilidadId { get; set; }

    public int PerfilId { get; set; }

    public int MascotaId { get; set; }

    public byte Puntaje { get; set; }

    public string? Factores { get; set; }

    public DateTime FechaCalculo { get; set; }

    public bool Activo { get; set; }

    public virtual Mascotas Mascota { get; set; } = null!;

    public virtual PerfilesAdoptante Perfil { get; set; } = null!;
}
