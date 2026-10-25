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
        // Repositorios
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ICatalogoRepository, CatalogoRepository>();
        services.AddScoped<IPerfilAdoptanteRepository, PerfilAdoptanteRepository>();
        services.AddScoped<IRefugioRepository, RefugioRepository>();
        services.AddScoped<IMascotaRepository, MascotaRepository>();
        // Servicios
        services.AddScoped<IHashContrasena, HashContrasena>();
        services.AddScoped<ICorreoService, CorreoService>();
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}