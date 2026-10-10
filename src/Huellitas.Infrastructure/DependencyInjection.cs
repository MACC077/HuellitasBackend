using Huellitas.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Huellitas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        //Registrar el DbContext con la cadena desde appsettings.json
        string connectionString = configuration.GetConnectionString("HuellitasDb") ??
        throw new InvalidOperationException("No se encontró la cadena de conexión 'HuellitasDb' en appsettings.json");

        services.AddDbContext<HuellitasDbContext>(options => options.UseSqlServer(connectionString));


        //Registrar repositorios y servicios externos

        return services;
    }
}