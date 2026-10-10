using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class RegistrosAuditorias
{
    public long AuditoriaId { get; set; }

    public int? UsuarioId { get; set; }

    public string Accion { get; set; } = null!;

    public string EntidadAfectada { get; set; } = null!;

    public int? IdEntidad { get; set; }

    public string? ValorAnterior { get; set; }

    public string? ValorNuevo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual Usuarios? Usuario { get; set; }
}
