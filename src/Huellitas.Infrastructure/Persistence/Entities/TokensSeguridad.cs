using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class TokensSeguridad
{
    public int TokenId { get; set; }

    public int UsuarioId { get; set; }

    public byte Tipo { get; set; }

    public string Valor { get; set; } = null!;

    public DateTime FechaExpiracion { get; set; }

    public bool Usado { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual Usuarios Usuario { get; set; } = null!;
}
