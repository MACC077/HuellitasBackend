using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class RefreshTokens
{
    public int RefreshTokenId { get; set; }
    public int UsuarioId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTime Expira { get; set; }
    public bool EstaRevocado { get; set; }
    public string? ReemplazadoPor { get; set; }
    public DateTime Creado { get; set; }
    public DateTime? RevocadoEn { get; set; }

    public virtual Usuarios Usuario { get; set; } = null!;
}