using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Usuarios
{
    public int UsuarioId { get; set; }

    public int RolId { get; set; }

    public int? RefugioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string ContrasenaHash { get; set; } = null!;

    public string? Telefono { get; set; }

    public byte Estado { get; set; }

    public byte IntentosFallidos { get; set; }

    public DateTime? FechaBloqueo { get; set; }

    public DateTime FechaRegistro { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<Apadrinamientos> Apadrinamientos { get; set; } = new List<Apadrinamientos>();

    public virtual ICollection<Calificaciones> Calificaciones { get; set; } = new List<Calificaciones>();

    public virtual ICollection<CambiosDeEstado> CambiosDeEstados { get; set; } = new List<CambiosDeEstado>();

    public virtual ICollection<ConsentimientosDatos> ConsentimientosDatos { get; set; } = new List<ConsentimientosDatos>();

    public virtual ICollection<Denuncias> Denuncia { get; set; } = new List<Denuncias>();

    public virtual ICollection<Favoritos> Favoritos { get; set; } = new List<Favoritos>();

    public virtual ICollection<Notificaciones> Notificaciones { get; set; } = new List<Notificaciones>();

    public virtual ICollection<PerfilesAdoptante> PerfilesAdoptantes { get; set; } = new List<PerfilesAdoptante>();

    public virtual Refugios? Refugio { get; set; }

    public virtual ICollection<Refugios> Refugios { get; set; } = new List<Refugios>();

    public virtual ICollection<RegistrosAuditorias> RegistrosAuditoria { get; set; } = new List<RegistrosAuditorias>();

    public virtual ICollection<ReportesCiudadano> ReportesCiudadanos { get; set; } = new List<ReportesCiudadano>();

    public virtual Roles Rol { get; set; } = null!;

    public virtual ICollection<SolicitudesAdopcion> SolicitudesAdopcions { get; set; } = new List<SolicitudesAdopcion>();

    public virtual ICollection<TokensSeguridad> TokensSeguridads { get; set; } = new List<TokensSeguridad>();
}
