using Huellitas.Application.DTOs.Administracion;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.Application.Mappings;

public static class AdminMappings
{
    public static UsuarioAdminDTO ToAdminDto(this Usuarios usuario)
    {
        return new UsuarioAdminDTO
        {
            UsuarioId = usuario.UsuarioId,
            Nombre = usuario.Nombre,
            Correo = usuario.Correo,
            Rol = usuario.Rol?.Nombre ?? string.Empty,
            Estado = NombreEstadoUsuario(usuario.Estado),
            Activo = usuario.Activo,
            FechaRegistro = usuario.FechaRegistro
        };
    }

    public static DenunciaDTO ToDto(this Denuncias denuncia)
    {
        return new DenunciaDTO
        {
            DenunciaId = denuncia.DenunciaId,
            Denunciante = denuncia.Usuario?.Nombre ?? string.Empty,
            TipoContenido = NombreTipoContenido(denuncia.TipoContenido),
            IdContenido = denuncia.IdContenido,
            Motivo = NombreMotivo(denuncia.Motivo),
            Descripcion = denuncia.Descripcion,
            Estado = NombreEstadoDenuncia(denuncia.Estado),
            Resultado = denuncia.Resultado,
            Fecha = denuncia.Fecha
        };
    }

    public static ConfiguracionDTO ToDto(this ConfiguracionSistema configuracion, bool enmascararValor)
    {
        return new ConfiguracionDTO
        {
            ConfiguracionId = configuracion.ConfiguracionId,
            Clave = configuracion.Clave,
            Valor = enmascararValor ? "***" : configuracion.Valor,
            Descripcion = configuracion.Descripcion,
            Activo = configuracion.Activo
        };
    }

    private static string NombreEstadoUsuario(byte valor)
    {
        return Enum.IsDefined(typeof(EstadoUsuario), valor)
            ? ((EstadoUsuario)valor).ToString()
            : valor.ToString();
    }

    private static string NombreEstadoDenuncia(byte valor)
    {
        return Enum.IsDefined(typeof(EstadoDenuncia), valor)
            ? ((EstadoDenuncia)valor).ToString()
            : valor.ToString();
    }

    private static string NombreTipoContenido(byte valor)
    {
        return Enum.IsDefined(typeof(TipoContenidoDenuncia), valor)
            ? ((TipoContenidoDenuncia)valor).ToString()
            : valor.ToString();
    }

    private static string NombreMotivo(byte valor)
    {
        return Enum.IsDefined(typeof(MotivoDenuncia), valor)
            ? ((MotivoDenuncia)valor).ToString()
            : valor.ToString();
    }
}
