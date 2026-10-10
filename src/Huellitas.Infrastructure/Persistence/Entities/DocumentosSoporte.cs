using System;
using System.Collections.Generic;

namespace Huellitas.Infrastructure.Persistence.Entities;

public partial class DocumentosSoporte
{
    public int DocumentoId { get; set; }

    public int SolicitudId { get; set; }

    public string Tipo { get; set; } = null!;

    public string Archivo { get; set; } = null!;

    public DateTime FechaCarga { get; set; }

    public bool Activo { get; set; }

    public virtual SolicitudesAdopcion Solicitud { get; set; } = null!;
}
