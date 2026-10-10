using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class CambiosDeEstado
{
    public long CambioId { get; set; }

    public int? UsuarioId { get; set; }

    public string Entidad { get; set; } = null!;

    public int IdEntidad { get; set; }

    public string? EstadoAnterior { get; set; }

    public string EstadoNuevo { get; set; } = null!;

    public string? Observacion { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual Usuarios? Usuario { get; set; }
}
