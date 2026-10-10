using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class PerfilesAdoptante
{
    public int PerfilId { get; set; }

    public int UsuarioId { get; set; }

    public byte? TipoVivienda { get; set; }

    public string? EspacioDisponible { get; set; }

    public bool? PresenciaDeNinos { get; set; }

    public bool? OtrasMascotas { get; set; }

    public string? TiempoDisponible { get; set; }

    public bool? ExperienciaPrevia { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<Compatibilidades> Compatibilidades { get; set; } = new List<Compatibilidades>();

    public virtual Usuarios Usuario { get; set; } = null!;
}
