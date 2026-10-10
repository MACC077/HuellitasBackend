namespace Huellitas.Infrastructure.Enums;

/// <summary>
/// Estado del usuario. Corresponde a Usuarios.Estado (TINYINT).
/// Definido en Figura 25 del documento de tesis.
/// </summary>
public enum EstadoUsuario : byte
{
    PendienteVerificacion = 0,
    Activo = 1,
    Bloqueado = 2,
    Desactivado = 3
}