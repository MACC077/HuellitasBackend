using Huellitas.Infrastructure.Interfaces.Services;

namespace Huellitas.Infrastructure.Services;

public class HashContrasena : IHashContrasena
{
    public string Hashear(string contrasena) => BCrypt.Net.BCrypt.HashPassword(contrasena);
    public bool Verificar(string contrasena, string hash) => BCrypt.Net.BCrypt.Verify(contrasena, hash);

}