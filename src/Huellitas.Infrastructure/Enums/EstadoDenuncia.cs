namespace Huellitas.Infrastructure.Enums;

/// <summary>
/// Estados de una denuncia. Corresponde a Denuncias.Estado (TINYINT).
/// </summary>
public enum EstadoDenuncia : byte
{
    Recibida = 0,
    EnRevision = 1,
    Resuelta = 2
}
