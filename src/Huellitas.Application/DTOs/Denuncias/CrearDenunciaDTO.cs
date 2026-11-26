namespace Huellitas.Application.DTOs.Denuncias;

public class CrearDenunciaDTO
{
    /// <summary>0 = Mascota, 1 = Refugio (perfil), 2 = Calificación (comentario), 3 = Reporte.</summary>
    public byte TipoContenido { get; set; }
    public int IdContenido { get; set; }
    /// <summary>0 = PosibleEstafa, 1 = InformacionFalsa, 2 = ContenidoInapropiado, 3 = Otro.</summary>
    public byte Motivo { get; set; }
    public string? Descripcion { get; set; }
}
