using brevo_csharp.Api;
using brevo_csharp.Model;
using Huellitas.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using BrevoConfig = brevo_csharp.Client.Configuration;


namespace Huellitas.Infrastructure.Services;

public class CorreoService : ICorreoService
{
    private readonly IConfiguration _config;
    public CorreoService(IConfiguration config) => _config = config;

    async System.Threading.Tasks.Task ICorreoService.EnviarCorreoVerificacionAsync(string destinatario, string nombre, string token)
    {
        var apiKey = _config["Brevo:ApiKey"]!;
        var remitenteEmail = _config["Brevo:RemitenteEmail"] ?? "no-reply@huellitas.com";
        var remitenteNombre = _config["Brevo:RemitenteNombre"] ?? "Huellitas";

        BrevoConfig.Default.ApiKey.Clear();
        BrevoConfig.Default.ApiKey.Add("api-key", apiKey);

        var apiInstance = new TransactionalEmailsApi();
        var enlace = $"https://localhost:7296/api/v1/Usuarios/confirmar?token={token}";

        var html = $@"
            <h2>¡Hola {nombre}!</h2>
            <p>Gracias por registrarte en <strong>Huellitas</strong>.</p>
            <p>Confirma tu cuenta en este enlace:</p>
            <p><a href='{enlace}'>Confirmar mi correo</a></p>";

        var email = new SendSmtpEmail
        {
            Sender = new SendSmtpEmailSender(remitenteNombre, remitenteEmail),
            To = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(destinatario, nombre) },
            Subject = "Confirma tu correo en Huellitas",
            HtmlContent = html
        };

        await apiInstance.SendTransacEmailAsync(email);
    }
}