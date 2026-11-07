namespace Huellitas.Infrastructure.Interfaces.Services;

public interface IPdfService
{
    byte[] GenerarConstanciaAdopcion(
        string adoptanteNombre,
        string adoptanteCorreo,
        string mascotaNombre,
        string refugioNombre,
        string codigoConstancia,
        DateTime fechaFinalizacion);
}