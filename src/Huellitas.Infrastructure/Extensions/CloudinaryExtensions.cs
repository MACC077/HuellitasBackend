using CloudinaryDotNet;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Huellitas.Infrastructure.Extensions;

public static class CloudinaryExtensions
{
    public static IServiceCollection AddCloudinaryService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // La documentación oficial de Cloudinary recomienda registrarlo como Singleton
        services.AddSingleton(serviceProvider =>
        {
            var cloudinaryConfig = configuration.GetSection("Cloudinary");

            var account = new Account(
                cloudinaryConfig["CloudName"],
                cloudinaryConfig["ApiKey"],
                cloudinaryConfig["ApiSecret"]
            );

            var cloudinary = new Cloudinary(account);
            cloudinary.Api.Secure = true; // Forzar HTTPS

            return cloudinary;
        });

        return services;
    }
}