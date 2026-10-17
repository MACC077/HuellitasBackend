using FluentValidation;
using Huellitas.Application.Interfaces.Services;
using Huellitas.Application.Services;
using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Persistence.Repositories;
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
        return services;
    }
}