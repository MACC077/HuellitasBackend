using System;
using System.Collections.Generic;
using Huellitas.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Huellitas.Infrastructure.Persistence.Context;

public partial class HuellitasDbContext : DbContext
{
    public HuellitasDbContext(DbContextOptions<HuellitasDbContext> options)
        : base(options)
    {
    }


    public virtual DbSet<Adopciones> Adopciones { get; set; }

    public virtual DbSet<Apadrinamientos> Apadrinamientos { get; set; }

    public virtual DbSet<Bitacoras> Bitacoras { get; set; }

    public virtual DbSet<Calificaciones> Calificaciones { get; set; }

    public virtual DbSet<CambiosDeEstado> CambiosDeEstados { get; set; }

    public virtual DbSet<Colores> Colores { get; set; }

    public virtual DbSet<Compatibilidades> Compatibilidades { get; set; }

    public virtual DbSet<ConfiguracionSistema> ConfiguracionSistemas { get; set; }

    public virtual DbSet<ConsentimientosDatos> ConsentimientosDatos { get; set; }

    public virtual DbSet<Denuncias> Denuncias { get; set; }

    public virtual DbSet<DocumentosSoporte> DocumentosSoportes { get; set; }

    public virtual DbSet<Especies> Especies { get; set; }

    public virtual DbSet<Favoritos> Favoritos { get; set; }

    public virtual DbSet<Mascotas> Mascotas { get; set; }

    public virtual DbSet<Multimedia> Multimedia { get; set; }

    public virtual DbSet<Necesidades> Necesidades { get; set; }

    public virtual DbSet<Notificaciones> Notificaciones { get; set; }

    public virtual DbSet<PerfilesAdoptante> PerfilesAdoptantes { get; set; }

    public virtual DbSet<PlanesSeguimiento> PlanesSeguimientos { get; set; }

    public virtual DbSet<PoliticasDato> PoliticasDatos { get; set; }

    public virtual DbSet<Razas> Razas { get; set; }

    public virtual DbSet<Refugios> Refugios { get; set; }

    public virtual DbSet<RegistrosAuditorias> RegistrosAuditoria { get; set; }

    public virtual DbSet<RegistrosClinico> RegistrosClinicos { get; set; }

    public virtual DbSet<ReportesCiudadano> ReportesCiudadanos { get; set; }

    public virtual DbSet<RequisitosAdopcion> RequisitosAdopcions { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<Seguimientos> Seguimientos { get; set; }

    public virtual DbSet<SolicitudesAdopcion> SolicitudesAdopcions { get; set; }

    public virtual DbSet<Tamanos> Tamanos { get; set; }

    public virtual DbSet<Temperamentos> Temperamentos { get; set; }

    public virtual DbSet<TokensSeguridad> TokensSeguridads { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    public virtual DbSet<VisitasEntrevista> VisitasEntrevista { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Gestion de mapeo de entidades a tablas y relaciones, aplicando configuraciones desde la respetiva clase de configuracion
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HuellitasDbContext).Assembly);

        modelBuilder.Entity<Adopciones>(entity =>
        {
            entity.HasKey(e => e.AdopcionId);

            entity.Property(e => e.CodigoConstancia).HasMaxLength(40);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaFinalizacion).HasPrecision(0);

            entity.HasOne(d => d.Solicitud).WithMany(p => p.Adopciones)
                .HasForeignKey(d => d.SolicitudId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Adopciones_Solicitudes");
        });

        modelBuilder.Entity<Apadrinamientos>(entity =>
        {
            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);
            entity.Property(e => e.Observacion).HasMaxLength(500);

            entity.HasOne(d => d.Mascota).WithMany(p => p.Apadrinamientos)
                .HasForeignKey(d => d.MascotaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Apadrinamientos_Mascotas");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Apadrinamientos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Apadrinamientos_Usuarios");
        });

        modelBuilder.Entity<Bitacoras>(entity =>
        {
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);

            entity.HasOne(d => d.Mascota).WithMany(p => p.Bitacoras)
                .HasForeignKey(d => d.MascotaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Bitacoras_Mascotas");
        });

        modelBuilder.Entity<Calificaciones>(entity =>
        {
            entity.HasKey(e => e.CalificacionId);

            entity.Property(e => e.Comentario).HasMaxLength(1000);
            entity.Property(e => e.Fecha).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);

            entity.HasOne(d => d.Adopcion).WithMany(p => p.Calificaciones)
                .HasForeignKey(d => d.AdopcionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Calificaciones_Adopciones");

            entity.HasOne(d => d.Refugio).WithMany(p => p.Calificaciones)
                .HasForeignKey(d => d.RefugioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Calificaciones_Refugios");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Calificaciones)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Calificaciones_Usuarios");
        });

        modelBuilder.Entity<CambiosDeEstado>(entity =>
        {
            entity.HasKey(e => e.CambioId);

            entity.ToTable("CambiosDeEstado");

            entity.Property(e => e.Entidad).HasMaxLength(40);
            entity.Property(e => e.EstadoAnterior).HasMaxLength(40);
            entity.Property(e => e.EstadoNuevo).HasMaxLength(40);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.Observacion).HasMaxLength(500);

            entity.HasOne(d => d.Usuario).WithMany(p => p.CambiosDeEstados)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_Cambios_Usuarios");
        });

        modelBuilder.Entity<Colores>(entity =>
        {
            entity.HasKey(e => e.ColorId);

            entity.Property(e => e.Nombre).HasMaxLength(40);
        });

        modelBuilder.Entity<Compatibilidades>(entity =>
        {
            entity.HasKey(e => e.CompatibilidadId);

            entity.Property(e => e.FechaCalculo).HasPrecision(0);

            entity.HasOne(d => d.Mascota).WithMany(p => p.Compatibilidades)
                .HasForeignKey(d => d.MascotaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compatibilidades_Mascotas");

            entity.HasOne(d => d.Perfil).WithMany(p => p.Compatibilidades)
                .HasForeignKey(d => d.PerfilId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Compatibilidades_Perfiles");
        });

        modelBuilder.Entity<ConfiguracionSistema>(entity =>
        {
            entity.HasKey(e => e.ConfiguracionId);

            entity.ToTable("ConfiguracionSistema");

            entity.Property(e => e.Clave).HasMaxLength(80);
            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);
            entity.Property(e => e.Valor).HasMaxLength(400);
        });

        modelBuilder.Entity<ConsentimientosDatos>(entity =>
        {
            entity.HasKey(e => e.ConsentimientoId);

            entity.Property(e => e.FechaAceptacion).HasPrecision(0);
            entity.Property(e => e.VersionAceptada).HasMaxLength(20);

            entity.HasOne(d => d.Politica).WithMany(p => p.ConsentimientosDatos)
                .HasForeignKey(d => d.PoliticaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Consentimientos_Politicas");

            entity.HasOne(d => d.Usuario).WithMany(p => p.ConsentimientosDatos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Consentimientos_Usuarios");
        });

        modelBuilder.Entity<Denuncias>(entity =>
        {
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.Fecha).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);
            entity.Property(e => e.Resultado).HasMaxLength(500);

            entity.HasOne(d => d.Usuario).WithMany(p => p.Denuncia)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Denuncias_Usuarios");
        });

        modelBuilder.Entity<DocumentosSoporte>(entity =>
        {
            entity.HasKey(e => e.DocumentoId);

            entity.ToTable("DocumentosSoporte");

            entity.Property(e => e.Archivo).HasMaxLength(400);
            entity.Property(e => e.FechaCarga).HasPrecision(0);
            entity.Property(e => e.Tipo).HasMaxLength(60);

            entity.HasOne(d => d.Solicitud).WithMany(p => p.DocumentosSoportes)
                .HasForeignKey(d => d.SolicitudId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Documentos_Solicitudes");
        });

        modelBuilder.Entity<Especies>(entity =>
        {
            entity.HasKey(e => e.EspecieId);

            entity.Property(e => e.Nombre).HasMaxLength(60);
        });

        modelBuilder.Entity<Favoritos>(entity =>
        {
            entity.Property(e => e.Fecha).HasPrecision(0);

            entity.HasOne(d => d.Mascota).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.MascotaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favoritos_Mascotas");

            entity.HasOne(d => d.Usuario).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favoritos_Usuarios");
        });

        modelBuilder.Entity<Mascotas>(entity =>
        {
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);
            entity.Property(e => e.FechaUltimaActualizacion).HasPrecision(0);
            entity.Property(e => e.MotivoBaja).HasMaxLength(300);
            entity.Property(e => e.Nombre).HasMaxLength(80);

            entity.HasOne(d => d.Color).WithMany(p => p.Mascota)
                .HasForeignKey(d => d.ColorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mascotas_Colores");

            entity.HasOne(d => d.Especie).WithMany(p => p.Mascota)
                .HasForeignKey(d => d.EspecieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mascotas_Especies");

            entity.HasOne(d => d.Raza).WithMany(p => p.Mascota)
                .HasForeignKey(d => d.RazaId)
                .HasConstraintName("FK_Mascotas_Razas");

            entity.HasOne(d => d.Refugio).WithMany(p => p.Mascota)
                .HasForeignKey(d => d.RefugioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mascotas_Refugios");

            entity.HasOne(d => d.Tamano).WithMany(p => p.Mascota)
                .HasForeignKey(d => d.TamanoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Mascotas_Tamanos");

            entity.HasMany(d => d.Temperamentos).WithMany(p => p.Mascota)
                .UsingEntity<Dictionary<string, object>>(
                    "MascotaTemperamento",
                    r => r.HasOne<Temperamentos>().WithMany()
                        .HasForeignKey("TemperamentoId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_MascotaTemp_Temperamentos"),
                    l => l.HasOne<Mascotas>().WithMany()
                        .HasForeignKey("MascotaId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_MascotaTemp_Mascotas"),
                    j =>
                    {
                        j.HasKey("MascotaId", "TemperamentoId");
                        j.ToTable("MascotaTemperamento");
                    });
        });

        modelBuilder.Entity<Multimedia>(entity =>
        {
            entity.HasKey(e => e.MultimediaId);

            entity.Property(e => e.Archivo).HasMaxLength(400);
            entity.Property(e => e.FechaCarga).HasPrecision(0);

            entity.HasOne(d => d.Mascota).WithMany(p => p.Multimedia)
                .HasForeignKey(d => d.MascotaId)
                .HasConstraintName("FK_Multimedia_Mascotas");

            entity.HasOne(d => d.Reporte).WithMany(p => p.Multimedia)
                .HasForeignKey(d => d.ReporteId)
                .HasConstraintName("FK_Multimedia_Reportes");

            entity.HasOne(d => d.Seguimiento).WithMany(p => p.Multimedia)
                .HasForeignKey(d => d.SeguimientoId)
                .HasConstraintName("FK_Multimedia_Seguimientos");
        });

        modelBuilder.Entity<Necesidades>(entity =>
        {
            entity.HasKey(e => e.NecesidadId);

            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);
            entity.Property(e => e.FechaPublicacion).HasPrecision(0);
            entity.Property(e => e.MedioDonacion).HasMaxLength(300);

            entity.HasOne(d => d.Refugio).WithMany(p => p.Necesidades)
                .HasForeignKey(d => d.RefugioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Necesidades_Refugios");
        });

        modelBuilder.Entity<Notificaciones>(entity =>
        {
            entity.HasKey(e => e.NotificacionId);

            entity.Property(e => e.FechaEnvio).HasPrecision(0);
            entity.Property(e => e.Mensaje).HasMaxLength(500);
            entity.Property(e => e.Tipo).HasMaxLength(60);

            entity.HasOne(d => d.Usuario).WithMany(p => p.Notificaciones)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notificaciones_Usuarios");
        });

        modelBuilder.Entity<PerfilesAdoptante>(entity =>
        {
            entity.HasKey(e => e.PerfilId);

            entity.ToTable("PerfilesAdoptante");

            entity.Property(e => e.EspacioDisponible).HasMaxLength(80);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);
            entity.Property(e => e.TiempoDisponible).HasMaxLength(80);

            entity.HasOne(d => d.Usuario).WithMany(p => p.PerfilesAdoptantes)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Perfiles_Usuarios");
        });

        modelBuilder.Entity<PlanesSeguimiento>(entity =>
        {
            entity.HasKey(e => e.PlanId);

            entity.ToTable("PlanesSeguimiento");

            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.Periodicidad).HasMaxLength(40);

            entity.HasOne(d => d.Adopcion).WithMany(p => p.PlanesSeguimientos)
                .HasForeignKey(d => d.AdopcionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Planes_Adopciones");
        });

        modelBuilder.Entity<PoliticasDato>(entity =>
        {
            entity.HasKey(e => e.PoliticaId);

            entity.Property(e => e.FechaPublicacion).HasPrecision(0);
            entity.Property(e => e.Version).HasMaxLength(20);
        });

        modelBuilder.Entity<Razas>(entity =>
        {
            entity.Property(e => e.Nombre).HasMaxLength(80);

            entity.HasOne(d => d.Especie).WithMany(p => p.Razas)
                .HasForeignKey(d => d.EspecieId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Razas_Especies");
        });

        modelBuilder.Entity<Refugios>(entity =>
        {
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.Direccion).HasMaxLength(200);
            entity.Property(e => e.Encargado).HasMaxLength(120);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);
            entity.Property(e => e.FechaVerificacion).HasPrecision(0);
            entity.Property(e => e.MotivoRechazo).HasMaxLength(500);
            entity.Property(e => e.Nit).HasMaxLength(20);
            entity.Property(e => e.Nombre).HasMaxLength(150);
            entity.Property(e => e.RedesSociales).HasMaxLength(300);
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(d => d.VerificadoPor).WithMany(p => p.Refugios)
                .HasForeignKey(d => d.VerificadoPorId)
                .HasConstraintName("FK_Refugios_VerificadoPor");
        });

        modelBuilder.Entity<RegistrosAuditorias>(entity =>
        {
            entity.HasKey(e => e.AuditoriaId);

            entity.Property(e => e.Accion).HasMaxLength(40);
            entity.Property(e => e.EntidadAfectada).HasMaxLength(60);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);

            entity.HasOne(d => d.Usuario).WithMany(p => p.RegistrosAuditoria)
                .HasForeignKey(d => d.UsuarioId)
                .HasConstraintName("FK_Auditoria_Usuarios");
        });

        modelBuilder.Entity<RegistrosClinico>(entity =>
        {
            entity.HasKey(e => e.RegistroClinicoId);

            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);
            entity.Property(e => e.SoporteVeterinario).HasMaxLength(400);

            entity.HasOne(d => d.Mascota).WithMany(p => p.RegistrosClinicos)
                .HasForeignKey(d => d.MascotaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_RegClinicos_Mascotas");
        });

        modelBuilder.Entity<ReportesCiudadano>(entity =>
        {
            entity.HasKey(e => e.ReporteId);

            entity.Property(e => e.Descripcion).HasMaxLength(1000);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);
            entity.Property(e => e.FechaReporte).HasPrecision(0);
            entity.Property(e => e.MotivoCierre).HasMaxLength(500);
            entity.Property(e => e.NumeroReporte).HasMaxLength(20);
            entity.Property(e => e.Ubicacion_Direccion).HasMaxLength(200);
            entity.Property(e => e.Ubicacion_Latitud).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Ubicacion_Longitud).HasColumnType("decimal(9, 6)");
            entity.Property(e => e.Ubicacion_PuntoReferencia).HasMaxLength(200);

            entity.HasOne(d => d.Mascota).WithMany(p => p.ReportesCiudadanos)
                .HasForeignKey(d => d.MascotaId)
                .HasConstraintName("FK_Reportes_Mascotas");

            entity.HasOne(d => d.Refugio).WithMany(p => p.ReportesCiudadanos)
                .HasForeignKey(d => d.RefugioId)
                .HasConstraintName("FK_Reportes_Refugios");

            entity.HasOne(d => d.Usuario).WithMany(p => p.ReportesCiudadanos)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reportes_Usuarios");
        });

        modelBuilder.Entity<RequisitosAdopcion>(entity =>
        {
            entity.HasKey(e => e.RequisitoId);

            entity.ToTable("RequisitosAdopcion");

            entity.Property(e => e.Descripcion).HasMaxLength(300);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);

            entity.HasOne(d => d.Refugio).WithMany(p => p.RequisitosAdopcions)
                .HasForeignKey(d => d.RefugioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Requisitos_Refugios");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.RolId);

            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Seguimientos>(entity =>
        {
            entity.Property(e => e.Adaptacion).HasMaxLength(300);
            entity.Property(e => e.Comentarios).HasMaxLength(1000);
            entity.Property(e => e.EstadoSalud).HasMaxLength(300);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaEnvio).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);

            entity.HasOne(d => d.Plan).WithMany(p => p.Seguimientos)
                .HasForeignKey(d => d.PlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Seguimientos_Planes");
        });

        modelBuilder.Entity<SolicitudesAdopcion>(entity =>
        {
            entity.HasKey(e => e.SolicitudId);

            entity.ToTable("SolicitudesAdopcion");

            entity.Property(e => e.Codigo).HasMaxLength(20);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaEnvio).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);
            entity.Property(e => e.MotivoRechazo).HasMaxLength(500);
            entity.Property(e => e.Observaciones).HasMaxLength(1000);

            entity.HasOne(d => d.Mascota).WithMany(p => p.SolicitudesAdopcions)
                .HasForeignKey(d => d.MascotaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Solicitudes_Mascotas");

            entity.HasOne(d => d.Usuario).WithMany(p => p.SolicitudesAdopcions)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Solicitudes_Usuarios");
        });

        modelBuilder.Entity<Tamanos>(entity =>
        {
            entity.Property(e => e.Nombre).HasMaxLength(40);
        });

        modelBuilder.Entity<Temperamentos>(entity =>
        {
            entity.Property(e => e.Nombre).HasMaxLength(60);
        });

        modelBuilder.Entity<TokensSeguridad>(entity =>
        {
            entity.HasKey(e => e.TokenId);

            entity.ToTable("TokensSeguridad");

            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaExpiracion).HasPrecision(0);
            entity.Property(e => e.Valor).HasMaxLength(255);

            entity.HasOne(d => d.Usuario).WithMany(p => p.TokensSeguridads)
                .HasForeignKey(d => d.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tokens_Usuarios");
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.Property(e => e.ContrasenaHash).HasMaxLength(255);
            entity.Property(e => e.Correo).HasMaxLength(150);
            entity.Property(e => e.FechaBloqueo).HasPrecision(0);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);
            entity.Property(e => e.FechaRegistro).HasPrecision(0);
            entity.Property(e => e.Nombre).HasMaxLength(120);
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(d => d.Refugio).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RefugioId)
                .HasConstraintName("FK_Usuarios_Refugios");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles");
        });

        modelBuilder.Entity<VisitasEntrevista>(entity =>
        {
            entity.HasKey(e => e.VisitaId);

            entity.Property(e => e.Fecha).HasPrecision(0);
            entity.Property(e => e.FechaCreacion).HasPrecision(0);
            entity.Property(e => e.FechaModificacion).HasPrecision(0);

            entity.HasOne(d => d.Solicitud).WithMany(p => p.VisitasEntrevista)
                .HasForeignKey(d => d.SolicitudId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Visitas_Solicitudes");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
