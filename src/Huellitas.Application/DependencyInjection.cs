using FluentValidation;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Huellitas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registrar los Services.
        var assembly = Assembly.GetExecutingAssembly();
        services.AddValidatorsFromAssembly(assembly);
        services.AddScoped<IAutenticacionService, AutenticacionService>();
        services.AddScoped<ICatalogoService, CatalogoService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IMascotaService, MascotaService>();
        services.AddScoped<IAdopcionService, AdopcionService>();
        services.AddScoped<IArchivoService, ArchivoService>();
        services.AddScoped<IRefugioService, RefugioService>();
        services.AddScoped<IFavoritoService, FavoritoService>();
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<ICalificacionService, CalificacionService>();
        services.AddScoped<INecesidadService, NecesidadService>();
        services.AddScoped<IApadrinamientoService, ApadrinamientoService>();
        services.AddScoped<ITrazabilidadService, TrazabilidadService>();
        services.AddScoped<INotificacionService, NotificacionService>();
        services.AddScoped<ISeguimientoService, SeguimientoService>();
        return services;
    }
}