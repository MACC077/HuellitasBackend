using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Huellitas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registrar AutoMapper, FluentValidation y los Services.
        // Por ahora queda vacío.
        var assembly = Assembly.GetExecutingAssembly();
        services.AddValidatorsFromAssembly(assembly);
        return services;
    }
}