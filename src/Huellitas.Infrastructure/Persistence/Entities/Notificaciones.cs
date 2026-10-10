using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Notificaciones
{
    public long NotificacionId { get; set; }

    public int UsuarioId { get; set; }

    public string Tipo { get; set; } = null!;

    public string Mensaje { get; set; } = null!;

    public byte Canal { get; set; }

    public DateTime FechaEnvio { get; set; }

    public bool Leida { get; set; }

    public virtual Usuarios Usuario { get; set; } = null!;
}
