using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Favoritos
{
    public int FavoritoId { get; set; }

    public int UsuarioId { get; set; }

    public int MascotaId { get; set; }

    public DateTime Fecha { get; set; }

    public bool Activo { get; set; }

    public virtual Mascotas Mascota { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;
}
