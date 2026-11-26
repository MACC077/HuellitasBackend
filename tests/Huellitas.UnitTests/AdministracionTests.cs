using Huellitas.Application.Common.Exceptions;
using Huellitas.Application.DTOs.Administracion;
using Huellitas.Application.Services;
using Huellitas.Application.Validators;
using Huellitas.Infrastructure.Enums;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Entities;

namespace Huellitas.UnitTests;

public class AdministracionTests
{
    private const int AdminId = 1;

    // ─── Contratos de enums ─────────────────────────────────────────

    [Fact]
    public void Enums_CoincidenConLaBaseDeDatos()
    {
        Assert.Equal(0, (byte)EstadoDenuncia.Recibida);
        Assert.Equal(1, (byte)EstadoDenuncia.EnRevision);
        Assert.Equal(2, (byte)EstadoDenuncia.Resuelta);

        Assert.Equal(0, (byte)TipoContenidoDenuncia.Mascota);
        Assert.Equal(1, (byte)TipoContenidoDenuncia.Refugio);
        Assert.Equal(2, (byte)TipoContenidoDenuncia.Calificacion);
        Assert.Equal(3, (byte)TipoContenidoDenuncia.Reporte);

        Assert.Equal(0, (byte)MotivoDenuncia.PosibleEstafa);
        Assert.Equal(1, (byte)MotivoDenuncia.InformacionFalsa);
        Assert.Equal(2, (byte)MotivoDenuncia.ContenidoInapropiado);
        Assert.Equal(3, (byte)MotivoDenuncia.Otro);
    }

    // ─── Validadores ────────────────────────────────────────────────

    [Fact]
    public void CambiarEstadoValidator_ExigeMotivoAlDesactivar()
    {
        var validator = new CambiarEstadoUsuarioValidator();

        var sinMotivo = validator.Validate(new CambiarEstadoUsuarioDTO { Activo = false });
        var conMotivo = validator.Validate(new CambiarEstadoUsuarioDTO { Activo = false, Motivo = "Incumplimiento" });
        var activar = validator.Validate(new CambiarEstadoUsuarioDTO { Activo = true });

        Assert.False(sinMotivo.IsValid);
        Assert.True(conMotivo.IsValid);
        Assert.True(activar.IsValid);
    }

    [Fact]
    public void ResolverDenunciaValidator_ExigeResultado()
    {
        var validator = new ResolverDenunciaValidator();

        var vacio = validator.Validate(new ResolverDenunciaDTO { Resultado = "   " });
        var valido = validator.Validate(new ResolverDenunciaDTO { Resultado = "Contenido verificado" });

        Assert.False(vacio.IsValid);
        Assert.True(valido.IsValid);
    }

    [Fact]
    public void CrearConfiguracionValidator_RechazaClaveConCaracteresInvalidos()
    {
        var validator = new CrearConfiguracionValidator();

        var invalida = validator.Validate(new CrearConfiguracionDTO { Clave = "clave con espacios", Valor = "1" });
        var valida = validator.Validate(new CrearConfiguracionDTO { Clave = "adopcion.max_dias", Valor = "30" });

        Assert.False(invalida.IsValid);
        Assert.True(valida.IsValid);
    }

    // ─── ADM-01: Usuarios ───────────────────────────────────────────

    [Fact]
    public async Task ObtenerUsuarios_MapeaRolYEstadoYValidaPaginacion()
    {
        var repo = new FakeAdminRepository();
        repo.AgregarUsuario(2, "Ana", "ana@x.com", RolUsuario.Adoptante, EstadoUsuario.Activo);
        repo.AgregarUsuario(3, "Beto", "beto@x.com", RolUsuario.Refugio, EstadoUsuario.Desactivado, activo: false);
        var service = CrearServicio(repo);

        var resultado = await service.ObtenerUsuariosAsync(1, 10, "ana");

        Assert.Single(resultado.Items);
        Assert.Equal("Adoptante", resultado.Items[0].Rol);
        Assert.Equal("Activo", resultado.Items[0].Estado);
        Assert.Equal(1, resultado.TotalRegistros);

        await Assert.ThrowsAsync<AppException>(() => service.ObtenerUsuariosAsync(0, 10, null));
        await Assert.ThrowsAsync<AppException>(() => service.ObtenerUsuariosAsync(1, 101, null));
    }

    [Fact]
    public async Task CambiarEstado_DesactivaUsuarioYRegistraAuditoria()
    {
        var repo = new FakeAdminRepository();
        var usuario = repo.AgregarUsuario(5, "Ana", "ana@x.com", RolUsuario.Adoptante, EstadoUsuario.Activo);
        var traza = new FakeTrazabilidadRepository();
        var service = new AdminService(repo, traza);

        var cambio = await service.CambiarEstadoUsuarioAsync(
            5,
            new CambiarEstadoUsuarioDTO { Activo = false, Motivo = "Fraude" },
            AdminId);

        Assert.True(cambio);
        Assert.False(usuario.Activo);
        Assert.Equal((byte)EstadoUsuario.Desactivado, usuario.Estado);
        Assert.NotNull(usuario.FechaModificacion);
        var auditoria = Assert.Single(traza.Auditorias);
        Assert.Equal("CambiarEstadoUsuario", auditoria.Accion);
        Assert.Equal(5, auditoria.IdEntidad);
        Assert.Contains("Fraude", auditoria.ValorNuevo);
    }

    [Fact]
    public async Task CambiarEstado_ActivarReseteaIntentosYBloqueo()
    {
        var repo = new FakeAdminRepository();
        var usuario = repo.AgregarUsuario(5, "Ana", "ana@x.com", RolUsuario.Adoptante, EstadoUsuario.Desactivado, activo: false);
        usuario.IntentosFallidos = 3;
        usuario.FechaBloqueo = DateTime.Now.AddMinutes(10);
        var service = CrearServicio(repo);

        var cambio = await service.CambiarEstadoUsuarioAsync(
            5,
            new CambiarEstadoUsuarioDTO { Activo = true },
            AdminId);

        Assert.True(cambio);
        Assert.True(usuario.Activo);
        Assert.Equal((byte)EstadoUsuario.Activo, usuario.Estado);
        Assert.Equal(0, usuario.IntentosFallidos);
        Assert.Null(usuario.FechaBloqueo);
    }

    [Fact]
    public async Task CambiarEstado_SinCambiosDevuelveFalseYSinAuditoria()
    {
        var repo = new FakeAdminRepository();
        repo.AgregarUsuario(5, "Ana", "ana@x.com", RolUsuario.Adoptante, EstadoUsuario.Activo);
        var traza = new FakeTrazabilidadRepository();
        var service = new AdminService(repo, traza);

        var cambio = await service.CambiarEstadoUsuarioAsync(
            5,
            new CambiarEstadoUsuarioDTO { Activo = true },
            AdminId);

        Assert.False(cambio);
        Assert.Empty(traza.Auditorias);
    }

    [Fact]
    public async Task CambiarEstado_RechazaAutoDesactivacion()
    {
        var repo = new FakeAdminRepository();
        repo.AgregarUsuario(AdminId, "Admin", "admin@x.com", RolUsuario.Administrador, EstadoUsuario.Activo);
        repo.AgregarUsuario(6, "Otro", "otro@x.com", RolUsuario.Administrador, EstadoUsuario.Activo);
        var service = CrearServicio(repo);

        var ex = await Assert.ThrowsAsync<AppException>(() => service.CambiarEstadoUsuarioAsync(
            AdminId,
            new CambiarEstadoUsuarioDTO { Activo = false, Motivo = "Prueba" },
            AdminId));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task CambiarEstado_NoDesactivaAlUltimoAdministradorActivo()
    {
        var repo = new FakeAdminRepository();
        repo.AgregarUsuario(7, "Admin", "admin@x.com", RolUsuario.Administrador, EstadoUsuario.Activo);
        var service = CrearServicio(repo);

        var ex = await Assert.ThrowsAsync<AppException>(() => service.CambiarEstadoUsuarioAsync(
            7,
            new CambiarEstadoUsuarioDTO { Activo = false, Motivo = "Prueba" },
            AdminId));

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task CambiarEstado_UsuarioInexistenteDevuelve404()
    {
        var service = CrearServicio(new FakeAdminRepository());

        var ex = await Assert.ThrowsAsync<AppException>(() => service.CambiarEstadoUsuarioAsync(
            999,
            new CambiarEstadoUsuarioDTO { Activo = true },
            AdminId));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task CambiarRol_ActualizaRolYQuitaRefugioAlDejarDeSerRefugio()
    {
        var repo = new FakeAdminRepository();
        var usuario = repo.AgregarUsuario(8, "Refu", "refu@x.com", RolUsuario.Refugio, EstadoUsuario.Activo);
        usuario.RefugioId = 3;
        repo.AgregarRol((int)RolUsuario.Refugio, "Refugio");
        repo.AgregarRol((int)RolUsuario.Adoptante, "Adoptante");
        var traza = new FakeTrazabilidadRepository();
        var service = new AdminService(repo, traza);

        var cambio = await service.CambiarRolUsuarioAsync(
            8,
            new CambiarRolUsuarioDTO { NuevoRolId = (int)RolUsuario.Adoptante },
            AdminId);

        Assert.True(cambio);
        Assert.Equal((int)RolUsuario.Adoptante, usuario.RolId);
        Assert.Null(usuario.RefugioId);
        var auditoria = Assert.Single(traza.Auditorias);
        Assert.Equal("CambiarRolUsuario", auditoria.Accion);
        Assert.Contains("RefugioId eliminado", auditoria.ValorNuevo);
    }

    [Fact]
    public async Task CambiarRol_RechazaRolInexistenteOInactivo()
    {
        var repo = new FakeAdminRepository();
        repo.AgregarUsuario(8, "Ana", "ana@x.com", RolUsuario.Adoptante, EstadoUsuario.Activo);
        repo.AgregarRol(9, "Inactivo", activo: false);
        var service = CrearServicio(repo);

        var inexistente = await Assert.ThrowsAsync<AppException>(() => service.CambiarRolUsuarioAsync(
            8, new CambiarRolUsuarioDTO { NuevoRolId = 99 }, AdminId));
        var inactivo = await Assert.ThrowsAsync<AppException>(() => service.CambiarRolUsuarioAsync(
            8, new CambiarRolUsuarioDTO { NuevoRolId = 9 }, AdminId));

        Assert.Equal(400, inexistente.StatusCode);
        Assert.Equal(400, inactivo.StatusCode);
    }

    [Fact]
    public async Task CambiarRol_NoRetiraRolAlUltimoAdministradorActivo()
    {
        var repo = new FakeAdminRepository();
        repo.AgregarUsuario(7, "Admin", "admin@x.com", RolUsuario.Administrador, EstadoUsuario.Activo);
        repo.AgregarRol((int)RolUsuario.Adoptante, "Adoptante");
        var service = CrearServicio(repo);

        var ex = await Assert.ThrowsAsync<AppException>(() => service.CambiarRolUsuarioAsync(
            7,
            new CambiarRolUsuarioDTO { NuevoRolId = (int)RolUsuario.Adoptante },
            AdminId));

        Assert.Equal(409, ex.StatusCode);
    }

    // ─── ADM-02: Denuncias ──────────────────────────────────────────

    [Fact]
    public async Task ObtenerDenuncias_RechazaEstadoInvalido()
    {
        var service = CrearServicio(new FakeAdminRepository());

        var ex = await Assert.ThrowsAsync<AppException>(() => service.ObtenerDenunciasAsync(1, 10, 99));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task ResolverDenuncia_MarcaResueltaOcultaContenidoYAudita()
    {
        var repo = new FakeAdminRepository();
        var denuncia = repo.AgregarDenuncia(4, TipoContenidoDenuncia.Mascota, 12, EstadoDenuncia.Recibida);
        repo.ContenidoExistente[((byte)TipoContenidoDenuncia.Mascota, 12)] = true;
        var traza = new FakeTrazabilidadRepository();
        var service = new AdminService(repo, traza);

        await service.ResolverDenunciaAsync(
            4,
            new ResolverDenunciaDTO { Resultado = "Maltrato confirmado", OcultarContenido = true },
            AdminId);

        Assert.Equal((byte)EstadoDenuncia.Resuelta, denuncia.Estado);
        Assert.Equal("Maltrato confirmado", denuncia.Resultado);
        Assert.NotNull(denuncia.FechaModificacion);
        Assert.Single(repo.Ocultamientos);
        Assert.Equal((byte)TipoContenidoDenuncia.Mascota, repo.Ocultamientos[0].Tipo);
        Assert.Equal(12, repo.Ocultamientos[0].Id);
        var auditoria = Assert.Single(traza.Auditorias);
        Assert.Equal("ResolverDenuncia", auditoria.Accion);
    }

    [Fact]
    public async Task ResolverDenuncia_YaResueltaDevuelve409()
    {
        var repo = new FakeAdminRepository();
        repo.AgregarDenuncia(4, TipoContenidoDenuncia.Mascota, 12, EstadoDenuncia.Resuelta);
        var service = CrearServicio(repo);

        var ex = await Assert.ThrowsAsync<AppException>(() => service.ResolverDenunciaAsync(
            4,
            new ResolverDenunciaDTO { Resultado = "Otra vez" },
            AdminId));

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task ResolverDenuncia_ContenidoInexistenteDevuelve404YNoCambiaEstado()
    {
        var repo = new FakeAdminRepository();
        var denuncia = repo.AgregarDenuncia(4, TipoContenidoDenuncia.Mascota, 12, EstadoDenuncia.EnRevision);
        var service = CrearServicio(repo);

        var ex = await Assert.ThrowsAsync<AppException>(() => service.ResolverDenunciaAsync(
            4,
            new ResolverDenunciaDTO { Resultado = "Ocultar", OcultarContenido = true },
            AdminId));

        Assert.Equal(404, ex.StatusCode);
        Assert.Equal((byte)EstadoDenuncia.EnRevision, denuncia.Estado);
        Assert.Empty(repo.Ocultamientos);
    }

    [Fact]
    public async Task ResolverDenuncia_ConflictoConcurrenteDevuelve409()
    {
        var repo = new FakeAdminRepository();
        repo.AgregarDenuncia(4, TipoContenidoDenuncia.Mascota, 12, EstadoDenuncia.EnRevision);
        repo.ForzarConflictoResolucion = true;
        var service = CrearServicio(repo);

        var ex = await Assert.ThrowsAsync<AppException>(() => service.ResolverDenunciaAsync(
            4,
            new ResolverDenunciaDTO { Resultado = "Resolver" },
            AdminId));

        Assert.Equal(409, ex.StatusCode);
    }

    // ─── ADM-03: Configuración ──────────────────────────────────────

    [Fact]
    public async Task Configuracion_EnmascaraClavesSensibles()
    {
        var repo = new FakeAdminRepository();
        repo.Configuraciones.Add(new ConfiguracionSistema { ConfiguracionId = 1, Clave = "smtp.password", Valor = "secreto", Activo = true });
        repo.Configuraciones.Add(new ConfiguracionSistema { ConfiguracionId = 2, Clave = "adopcion.max_dias", Valor = "30", Activo = true });
        var service = CrearServicio(repo);

        var resultado = (await service.ObtenerConfiguracionesAsync()).ToList();

        Assert.Equal("***", resultado.Single(c => c.ConfiguracionId == 1).Valor);
        Assert.Equal("30", resultado.Single(c => c.ConfiguracionId == 2).Valor);
    }

    [Fact]
    public async Task Configuracion_RechazaClaveSensible()
    {
        var repo = new FakeAdminRepository();
        var service = CrearServicio(repo);

        var ex = await Assert.ThrowsAsync<AppException>(() => service.CrearConfiguracionAsync(
            new CrearConfiguracionDTO { Clave = "api_token", Valor = "abc" },
            AdminId));

        Assert.Equal(400, ex.StatusCode);
        Assert.Empty(repo.Configuraciones);
    }

    [Fact]
    public async Task Configuracion_RechazaClaveDuplicada()
    {
        var repo = new FakeAdminRepository();
        repo.Configuraciones.Add(new ConfiguracionSistema { ConfiguracionId = 1, Clave = "adopcion.max_dias", Valor = "30", Activo = true });
        var service = CrearServicio(repo);

        var ex = await Assert.ThrowsAsync<AppException>(() => service.CrearConfiguracionAsync(
            new CrearConfiguracionDTO { Clave = "adopcion.max_dias", Valor = "45" },
            AdminId));

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task Configuracion_CrearYActualizarAuditan()
    {
        var repo = new FakeAdminRepository();
        var traza = new FakeTrazabilidadRepository();
        var service = new AdminService(repo, traza);

        var id = await service.CrearConfiguracionAsync(
            new CrearConfiguracionDTO { Clave = "adopcion.max_dias", Valor = "30", Descripcion = "Límite" },
            AdminId);

        await service.ActualizarConfiguracionAsync(
            id,
            new CrearConfiguracionDTO { Clave = "adopcion.max_dias", Valor = "45", Descripcion = "Límite" },
            AdminId);

        Assert.Equal(2, traza.Auditorias.Count);
        Assert.Equal("CrearConfiguracion", traza.Auditorias[0].Accion);
        Assert.Equal("ActualizarConfiguracion", traza.Auditorias[1].Accion);
        Assert.Equal("45", repo.Configuraciones.Single(c => c.ConfiguracionId == id).Valor);
    }

    [Fact]
    public async Task Configuracion_ActualizarInexistenteDevuelve404()
    {
        var service = CrearServicio(new FakeAdminRepository());

        var ex = await Assert.ThrowsAsync<AppException>(() => service.ActualizarConfiguracionAsync(
            99,
            new CrearConfiguracionDTO { Clave = "clave", Valor = "1" },
            AdminId));

        Assert.Equal(404, ex.StatusCode);
    }

    // ─── ADM-04: Dashboard ──────────────────────────────────────────

    [Fact]
    public async Task Dashboard_MapeaMetricasYRedondeaPromedio()
    {
        var repo = new FakeAdminRepository
        {
            AdopcionesExitosas = 4,
            MascotasDisponibles = 7,
            UsuariosActivos = 10,
            RefugiosVerificados = 2,
            ReportesCiudadanos = 3,
            PromedioAdopcion = 12.345
        };
        var service = CrearServicio(repo);

        var metricas = await service.ObtenerMetricasAsync();

        Assert.Equal(4, metricas.TotalAdopcionesExitosas);
        Assert.Equal(7, metricas.TotalMascotasDisponibles);
        Assert.Equal(10, metricas.TotalUsuariosRegistrados);
        Assert.Equal(2, metricas.TotalRefugiosVerificados);
        Assert.Equal(3, metricas.TotalReportesCiudadanos);
        Assert.Equal(12.3, metricas.TiempoPromedioAdopcionDias);
    }

    // ─── Helpers ────────────────────────────────────────────────────

    private static AdminService CrearServicio(FakeAdminRepository repo)
        => new(repo, new FakeTrazabilidadRepository());

    private sealed class FakeTrazabilidadRepository : ITrazabilidadRepository
    {
        public List<RegistrosAuditorias> Auditorias { get; } = new();

        public Task InsertarAuditoriaAsync(RegistrosAuditorias auditoria)
        {
            Auditorias.Add(auditoria);
            return Task.CompletedTask;
        }

        public Task InsertarCambioEstadoAsync(CambiosDeEstado cambio) => Task.CompletedTask;
        public Task<IEnumerable<CambiosDeEstado>> ObtenerCambiosEstadoAsync(string entidad, int idEntidad)
            => Task.FromResult<IEnumerable<CambiosDeEstado>>(Array.Empty<CambiosDeEstado>());
        public Task InsertarBitacoraAsync(Bitacoras bitacora) => Task.CompletedTask;
        public Task<IEnumerable<Bitacoras>> ObtenerBitacorasAsync(int mascotaId)
            => Task.FromResult<IEnumerable<Bitacoras>>(Array.Empty<Bitacoras>());
        public Task<(IEnumerable<RegistrosAuditorias> Items, int Total)> ObtenerAuditoriaAsync(int pagina, int tamano)
            => Task.FromResult<(IEnumerable<RegistrosAuditorias>, int)>((Array.Empty<RegistrosAuditorias>(), 0));
    }

    private sealed class FakeAdminRepository : IAdminRepository
    {
        public List<Usuarios> Usuarios { get; } = new();
        public List<Denuncias> Denuncias { get; } = new();
        public List<ConfiguracionSistema> Configuraciones { get; } = new();
        public List<Roles> Roles { get; } = new();
        public Dictionary<(byte Tipo, int Id), bool> ContenidoExistente { get; } = new();
        public List<(byte Tipo, int Id, DateTime Fecha)> Ocultamientos { get; } = new();

        public int AdopcionesExitosas { get; set; }
        public int MascotasDisponibles { get; set; }
        public int UsuariosActivos { get; set; }
        public int RefugiosVerificados { get; set; }
        public int ReportesCiudadanos { get; set; }
        public double PromedioAdopcion { get; set; }
        public bool ForzarConflictoResolucion { get; set; }

        public Usuarios AgregarUsuario(int id, string nombre, string correo, RolUsuario rol, EstadoUsuario estado, bool activo = true)
        {
            var usuario = new Usuarios
            {
                UsuarioId = id,
                Nombre = nombre,
                Correo = correo,
                ContrasenaHash = "hash",
                RolId = (int)rol,
                Rol = new Roles { RolId = (int)rol, Nombre = rol.ToString(), Activo = true },
                Estado = (byte)estado,
                Activo = activo,
                FechaRegistro = DateTime.Now,
                FechaCreacion = DateTime.Now
            };
            Usuarios.Add(usuario);
            return usuario;
        }

        public void AgregarRol(int id, string nombre, bool activo = true)
            => Roles.Add(new Roles { RolId = id, Nombre = nombre, Activo = activo });

        public Denuncias AgregarDenuncia(int id, TipoContenidoDenuncia tipo, int idContenido, EstadoDenuncia estado)
        {
            var denuncia = new Denuncias
            {
                DenunciaId = id,
                UsuarioId = 10,
                Usuario = new Usuarios { UsuarioId = 10, Nombre = "Denunciante" },
                TipoContenido = (byte)tipo,
                IdContenido = idContenido,
                Motivo = (byte)MotivoDenuncia.PosibleEstafa,
                Descripcion = "Descripción",
                Estado = (byte)estado,
                Fecha = DateTime.Now,
                Activo = true
            };
            Denuncias.Add(denuncia);
            return denuncia;
        }

        public Task<(IEnumerable<Usuarios> Items, int Total)> ObtenerUsuariosAsync(int pagina, int tamano, string? busqueda)
        {
            var query = Usuarios.AsQueryable();
            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(u => u.Nombre.Contains(busqueda) || u.Correo.Contains(busqueda));

            var total = query.Count();
            var items = query.OrderBy(u => u.Nombre)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToList();

            return Task.FromResult<(IEnumerable<Usuarios>, int)>((items, total));
        }

        public Task<Usuarios?> ObtenerUsuarioConRolAsync(int usuarioId)
            => Task.FromResult(Usuarios.FirstOrDefault(u => u.UsuarioId == usuarioId));

        public Task<Roles?> ObtenerRolAsync(int rolId)
            => Task.FromResult(Roles.FirstOrDefault(r => r.RolId == rolId));

        public Task<int> ContarAdministradoresActivosAsync(int excluyendoUsuarioId)
            => Task.FromResult(Usuarios.Count(u =>
                u.UsuarioId != excluyendoUsuarioId &&
                u.RolId == (int)RolUsuario.Administrador &&
                u.Activo &&
                u.Estado == (byte)EstadoUsuario.Activo));

        public Task ActualizarUsuarioAsync(Usuarios usuario) => Task.CompletedTask;

        public Task<(IEnumerable<Denuncias> Items, int Total)> ObtenerDenunciasAsync(int pagina, int tamano, byte? estado)
        {
            var query = Denuncias.Where(d => d.Activo);
            if (estado.HasValue)
                query = query.Where(d => d.Estado == estado.Value);

            var lista = query.ToList();
            var items = lista.OrderByDescending(d => d.Fecha)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .ToList();

            return Task.FromResult<(IEnumerable<Denuncias>, int)>((items, lista.Count));
        }

        public Task<Denuncias?> ObtenerDenunciaAsync(int denunciaId)
            => Task.FromResult(Denuncias.FirstOrDefault(d => d.DenunciaId == denunciaId && d.Activo));

        public Task<bool> MarcarDenunciaResueltaAsync(int denunciaId, string resultado, DateTime fecha)
        {
            if (ForzarConflictoResolucion)
                return Task.FromResult(false);

            var denuncia = Denuncias.FirstOrDefault(d => d.DenunciaId == denunciaId && d.Activo);

            if (denuncia is null || denuncia.Estado == (byte)EstadoDenuncia.Resuelta)
                return Task.FromResult(false);

            denuncia.Estado = (byte)EstadoDenuncia.Resuelta;
            denuncia.Resultado = resultado;
            denuncia.FechaModificacion = fecha;
            return Task.FromResult(true);
        }

        public Task<bool> ExisteContenidoAsync(byte tipoContenido, int idContenido)
            => Task.FromResult(ContenidoExistente.TryGetValue((tipoContenido, idContenido), out var existe) && existe);

        public Task<bool> OcultarContenidoAsync(byte tipoContenido, int idContenido, DateTime fecha)
        {
            Ocultamientos.Add((tipoContenido, idContenido, fecha));
            return Task.FromResult(true);
        }

        public Task<IEnumerable<ConfiguracionSistema>> ObtenerConfiguracionesAsync()
            => Task.FromResult<IEnumerable<ConfiguracionSistema>>(Configuraciones.OrderBy(c => c.Clave).ToList());

        public Task<ConfiguracionSistema?> ObtenerConfiguracionAsync(int configuracionId)
            => Task.FromResult(Configuraciones.FirstOrDefault(c => c.ConfiguracionId == configuracionId));

        public Task<bool> ExisteClaveConfiguracionAsync(string clave, int? excluyendoId)
            => Task.FromResult(Configuraciones.Any(c =>
                c.Clave == clave && (!excluyendoId.HasValue || c.ConfiguracionId != excluyendoId.Value)));

        public Task InsertarConfiguracionAsync(ConfiguracionSistema configuracion)
        {
            configuracion.ConfiguracionId = Configuraciones.Count == 0 ? 1 : Configuraciones.Max(c => c.ConfiguracionId) + 1;
            Configuraciones.Add(configuracion);
            return Task.CompletedTask;
        }

        public Task ActualizarConfiguracionAsync(ConfiguracionSistema configuracion) => Task.CompletedTask;

        public Task<int> ContarAdopcionesExitosasAsync() => Task.FromResult(AdopcionesExitosas);
        public Task<int> ContarMascotasDisponiblesAsync() => Task.FromResult(MascotasDisponibles);
        public Task<int> ContarUsuariosActivosAsync() => Task.FromResult(UsuariosActivos);
        public Task<int> ContarRefugiosVerificadosAsync() => Task.FromResult(RefugiosVerificados);
        public Task<int> ContarReportesCiudadanosAsync() => Task.FromResult(ReportesCiudadanos);
        public Task<double> ObtenerTiempoPromedioAdopcionDiasAsync() => Task.FromResult(PromedioAdopcion);
    }
}
