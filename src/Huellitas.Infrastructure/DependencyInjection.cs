using Huellitas.Infrastructure.Interfaces.Repositories;
using Huellitas.Infrastructure.Interfaces.Services;
using Huellitas.Infrastructure.Persistence.Context;
using Huellitas.Infrastructure.Persistence.Repositories;
using Huellitas.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Huellitas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        string cs = config.GetConnectionString("HuellitasDb")!;
        services.AddDbContext<HuellitasDbContext>(opt => opt.UseSqlServer(cs));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IHashContrasena, HashContrasena>();
        services.AddScoped<IProveedorTokens, ProveedorTokens>();
        services.AddScoped<ICorreoService, CorreoService>();

        return services;
    }
}