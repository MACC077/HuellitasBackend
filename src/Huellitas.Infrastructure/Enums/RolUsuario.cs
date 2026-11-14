namespace Huellitas.Infrastructure.Enums;

/// <summary>
/// Mapea exactamente los RolId de la tabla Roles en la base de datos.
/// </summary>
public enum RolUsuario : int
{
    Visitante = 1,
    Administrador = 2,
    Refugio = 3,
    Adoptante = 4
}