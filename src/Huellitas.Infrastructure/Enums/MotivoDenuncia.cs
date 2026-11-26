namespace Huellitas.Infrastructure.Enums;

/// <summary>
/// Motivo de una denuncia. Corresponde a Denuncias.Motivo (TINYINT).
/// </summary>
public enum MotivoDenuncia : byte
{
    PosibleEstafa = 0,
    InformacionFalsa = 1,
    ContenidoInapropiado = 2,
    Otro = 3
}
