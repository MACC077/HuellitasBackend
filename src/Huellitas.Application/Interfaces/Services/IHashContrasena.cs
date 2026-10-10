namespace Huellitas.Application.Interfaces.Services;

/// <summary>
/// Abstracción del hashing de contraseñas (RNF-02).
/// </summary>
public interface IHashContrasena
{
    string Hashear(string contrasena);
    bool Verificar(string contrasena, string hash);
}