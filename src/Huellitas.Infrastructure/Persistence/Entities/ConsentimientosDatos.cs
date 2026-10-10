using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class ConsentimientosDatos
{
    public int ConsentimientoId { get; set; }

    public int UsuarioId { get; set; }

    public int PoliticaId { get; set; }

    public string VersionAceptada { get; set; } = null!;

    public DateTime FechaAceptacion { get; set; }

    public bool Activo { get; set; }

    public virtual PoliticasDato Politica { get; set; } = null!;

    public virtual Usuarios Usuario { get; set; } = null!;
}
