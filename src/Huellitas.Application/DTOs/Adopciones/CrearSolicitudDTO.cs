namespace Huellitas.Application.DTOs.Adopciones;

public class CrearSolicitudDTO
{
    public int MascotaId { get; set; }
    public object RespuestasFormulario { get; set; } = string.Empty;
}