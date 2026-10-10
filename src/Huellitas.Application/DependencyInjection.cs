using Microsoft.Extensions.DependencyInjection;

namespace Huellitas.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Registrar AutoMapper, FluentValidation y los Services.
        // Por ahora queda vacío.

        return services;
    }
}