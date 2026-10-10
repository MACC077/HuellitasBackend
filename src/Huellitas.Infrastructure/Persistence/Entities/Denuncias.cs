using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Denuncias
{
    public int DenunciaId { get; set; }

    public int UsuarioId { get; set; }

    public byte TipoContenido { get; set; }

    public int IdContenido { get; set; }

    public byte Motivo { get; set; }

    public string? Descripcion { get; set; }

    public byte Estado { get; set; }

    public string? Resultado { get; set; }

    public DateTime Fecha { get; set; }

    public bool Activo { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Usuarios Usuario { get; set; } = null!;
}
