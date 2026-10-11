using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Huellitas.Infrastructure.Persistence.Configurations;

// ═══════════════════════════════════════════════════════════════
// Configuraciones de PRIMARY KEY para todas las entidades.
// Necesarias porque el scaffold no las detectó por el pluralizador
// del español (clase plural + PK singular).
// ═══════════════════════════════════════════════════════════════

public class AdopcionesConfiguration : IEntityTypeConfiguration<Adopciones>
{
    public void Configure(EntityTypeBuilder<Adopciones> b) => b.HasKey(x => x.AdopcionId);
}

public class ApadrinamientosConfiguration : IEntityTypeConfiguration<Apadrinamientos>
{
    public void Configure(EntityTypeBuilder<Apadrinamientos> b) => b.HasKey(x => x.ApadrinamientoId);
}

public class BitacorasConfiguration : IEntityTypeConfiguration<Bitacoras>
{
    public void Configure(EntityTypeBuilder<Bitacoras> b) => b.HasKey(x => x.BitacoraId);
}

public class CalificacionesConfiguration : IEntityTypeConfiguration<Calificaciones>
{
    public void Configure(EntityTypeBuilder<Calificaciones> b) => b.HasKey(x => x.CalificacionId);
}

public class CambiosDeEstadoConfiguration : IEntityTypeConfiguration<CambiosDeEstado>
{
    public void Configure(EntityTypeBuilder<CambiosDeEstado> b) => b.HasKey(x => x.CambioId);
}

public class ColoresConfiguration : IEntityTypeConfiguration<Colores>
{
    public void Configure(EntityTypeBuilder<Colores> b) => b.HasKey(x => x.ColorId);
}

public class CompatibilidadesConfiguration : IEntityTypeConfiguration<Compatibilidades>
{
    public void Configure(EntityTypeBuilder<Compatibilidades> b) => b.HasKey(x => x.CompatibilidadId);
}

public class ConfiguracionSistemaConfiguration : IEntityTypeConfiguration<ConfiguracionSistema>
{
    public void Configure(EntityTypeBuilder<ConfiguracionSistema> b) => b.HasKey(x => x.ConfiguracionId);
}

public class ConsentimientosDatosConfiguration : IEntityTypeConfiguration<ConsentimientosDatos>
{
    public void Configure(EntityTypeBuilder<ConsentimientosDatos> b) => b.HasKey(x => x.ConsentimientoId);
}

public class DenunciasConfiguration : IEntityTypeConfiguration<Denuncias>
{
    public void Configure(EntityTypeBuilder<Denuncias> b) => b.HasKey(x => x.DenunciaId);
}

public class DocumentosSoporteConfiguration : IEntityTypeConfiguration<DocumentosSoporte>
{
    public void Configure(EntityTypeBuilder<DocumentosSoporte> b) => b.HasKey(x => x.DocumentoId);
}

public class EspeciesConfiguration : IEntityTypeConfiguration<Especies>
{
    public void Configure(EntityTypeBuilder<Especies> b) => b.HasKey(x => x.EspecieId);
}

public class FavoritosConfiguration : IEntityTypeConfiguration<Favoritos>
{
    public void Configure(EntityTypeBuilder<Favoritos> b) => b.HasKey(x => x.FavoritoId);
}

public class MascotasConfiguration : IEntityTypeConfiguration<Mascotas>
{
    public void Configure(EntityTypeBuilder<Mascotas> b) => b.HasKey(x => x.MascotaId);
}

public class MultimediaConfiguration : IEntityTypeConfiguration<Multimedia>
{
    public void Configure(EntityTypeBuilder<Multimedia> b) => b.HasKey(x => x.MultimediaId);
}

public class NecesidadesConfiguration : IEntityTypeConfiguration<Necesidades>
{
    public void Configure(EntityTypeBuilder<Necesidades> b) => b.HasKey(x => x.NecesidadId);
}

public class NotificacionesConfiguration : IEntityTypeConfiguration<Notificaciones>
{
    public void Configure(EntityTypeBuilder<Notificaciones> b) => b.HasKey(x => x.NotificacionId);
}

public class PerfilesAdoptanteConfiguration : IEntityTypeConfiguration<PerfilesAdoptante>
{
    public void Configure(EntityTypeBuilder<PerfilesAdoptante> b) => b.HasKey(x => x.PerfilId);
}

public class PlanesSeguimientoConfiguration : IEntityTypeConfiguration<PlanesSeguimiento>
{
    public void Configure(EntityTypeBuilder<PlanesSeguimiento> b) => b.HasKey(x => x.PlanId);
}

public class PoliticasDatosConfiguration : IEntityTypeConfiguration<PoliticasDato>
{
    public void Configure(EntityTypeBuilder<PoliticasDato> b) => b.HasKey(x => x.PoliticaId);
}

public class RazasConfiguration : IEntityTypeConfiguration<Razas>
{
    public void Configure(EntityTypeBuilder<Razas> b) => b.HasKey(x => x.RazaId);
}

public class RefugiosConfiguration : IEntityTypeConfiguration<Refugios>
{
    public void Configure(EntityTypeBuilder<Refugios> b) => b.HasKey(x => x.RefugioId);
}

public class RegistrosAuditoriaConfiguration : IEntityTypeConfiguration<RegistrosAuditorias>
{
    public void Configure(EntityTypeBuilder<RegistrosAuditorias> b) => b.HasKey(x => x.AuditoriaId);
}

public class RegistrosClinicosConfiguration : IEntityTypeConfiguration<RegistrosClinico>
{
    public void Configure(EntityTypeBuilder<RegistrosClinico> b) => b.HasKey(x => x.RegistroClinicoId);
}

public class ReportesCiudadanosConfiguration : IEntityTypeConfiguration<ReportesCiudadano>
{
    public void Configure(EntityTypeBuilder<ReportesCiudadano> b) => b.HasKey(x => x.ReporteId);
}

public class RequisitosAdopcionConfiguration : IEntityTypeConfiguration<RequisitosAdopcion>
{
    public void Configure(EntityTypeBuilder<RequisitosAdopcion> b) => b.HasKey(x => x.RequisitoId);
}

public class RolesConfiguration : IEntityTypeConfiguration<Roles>
{
    public void Configure(EntityTypeBuilder<Roles> b) => b.HasKey(x => x.RolId);
}

public class SeguimientosConfiguration : IEntityTypeConfiguration<Seguimientos>
{
    public void Configure(EntityTypeBuilder<Seguimientos> b) => b.HasKey(x => x.SeguimientoId);
}

public class SolicitudesAdopcionConfiguration : IEntityTypeConfiguration<SolicitudesAdopcion>
{
    public void Configure(EntityTypeBuilder<SolicitudesAdopcion> b) => b.HasKey(x => x.SolicitudId);
}

public class TamanosConfiguration : IEntityTypeConfiguration<Tamanos>
{
    public void Configure(EntityTypeBuilder<Tamanos> b) => b.HasKey(x => x.TamanoId);
}

public class TemperamentosConfiguration : IEntityTypeConfiguration<Temperamentos>
{
    public void Configure(EntityTypeBuilder<Temperamentos> b) => b.HasKey(x => x.TemperamentoId);
}

public class VisitasEntrevistaConfiguration : IEntityTypeConfiguration<VisitasEntrevista>
{
    public void Configure(EntityTypeBuilder<VisitasEntrevista> b) => b.HasKey(x => x.VisitaId);
}