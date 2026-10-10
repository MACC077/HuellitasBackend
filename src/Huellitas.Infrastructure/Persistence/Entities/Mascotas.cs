using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class Mascotas
{
    public int MascotaId { get; set; }

    public int RefugioId { get; set; }

    public int EspecieId { get; set; }

    public int? RazaId { get; set; }

    public int TamanoId { get; set; }

    public int ColorId { get; set; }

    public string Nombre { get; set; } = null!;

    public byte Sexo { get; set; }

    public byte? EdadAproximada { get; set; }

    public string? Descripcion { get; set; }

    public bool Esterilizada { get; set; }

    public byte Estado { get; set; }

    public byte Procedencia { get; set; }

    public DateOnly FechaIngreso { get; set; }

    public DateTime FechaUltimaActualizacion { get; set; }

    public string? MotivoBaja { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<Apadrinamientos> Apadrinamientos { get; set; } = new List<Apadrinamientos>();

    public virtual ICollection<Bitacoras> Bitacoras { get; set; } = new List<Bitacoras>();

    public virtual Colores Color { get; set; } = null!;

    public virtual ICollection<Compatibilidades> Compatibilidades { get; set; } = new List<Compatibilidades>();

    public virtual Especies Especie { get; set; } = null!;

    public virtual ICollection<Favoritos> Favoritos { get; set; } = new List<Favoritos>();

    public virtual ICollection<Multimedia> Multimedia { get; set; } = new List<Multimedia>();

    public virtual Razas? Raza { get; set; }

    public virtual Refugios Refugio { get; set; } = null!;

    public virtual ICollection<RegistrosClinico> RegistrosClinicos { get; set; } = new List<RegistrosClinico>();

    public virtual ICollection<ReportesCiudadano> ReportesCiudadanos { get; set; } = new List<ReportesCiudadano>();

    public virtual ICollection<SolicitudesAdopcion> SolicitudesAdopcions { get; set; } = new List<SolicitudesAdopcion>();

    public virtual Tamanos Tamano { get; set; } = null!;

    public virtual ICollection<Temperamentos> Temperamentos { get; set; } = new List<Temperamentos>();
}
