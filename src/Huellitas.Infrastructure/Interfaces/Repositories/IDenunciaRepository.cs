using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Infrastructure.Interfaces.Repositories;

public interface IDenunciaRepository
{
    Task<Denuncias> InsertarAsync(Denuncias denuncia);
    Task<bool> ExisteContenidoAsync(byte tipoContenido, int idContenido);

    /// <summary>
    /// Evita que un mismo usuario acumule denuncias abiertas sobre el mismo contenido.
    /// </summary>
    Task<bool> ExisteDenunciaPendienteAsync(int usuarioId, byte tipoContenido, int idContenido);
}
