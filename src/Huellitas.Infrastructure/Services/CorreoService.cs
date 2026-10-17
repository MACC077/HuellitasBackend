using brevo_csharp.Api;
using brevo_csharp.Model;
using Huellitas.Infrastructure.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using BrevoConfig = brevo_csharp.Client.Configuration;
using Task = System.Threading.Tasks.Task;


namespace Huellitas.Infrastructure.Services;

public class CorreoService : ICorreoService
{
    private readonly IConfiguration _config;
    public CorreoService(IConfiguration config) => _config = config;
    private readonly string _urlCorreoDev = "https://localhost:7296";

    public async Task EnviarCorreoVerificacionAsync(string destinatario, string nombre, string token)
    {
        var baseUrl = _config["App:PublicBaseUrl"] ?? _urlCorreoDev;
        var verifyPath = _config["App:VerifyPath"] ?? "/api/v1/Usuarios/confirmar";
        var enlace = $"{baseUrl}{verifyPath}?token={token}";

        var html = $@"
            <h2>¡Hola {nombre}!</h2>
            <p>Gracias por registrarte en <strong>Huellitas</strong>.</p>
            <p>Para activar tu cuenta, haz clic en el siguiente enlace:</p>
            <p>
                <a href='{enlace}' style='display:inline-block; padding:12px 24px; background:#2E7D32; color:white; text-decoration:none; border-radius:5px; font-weight:bold;'>
                    Confirmar mi correo
                </a>
            </p>
            <p style='color:#666; font-size:12px;'>Si el botón no funciona, copia este enlace en tu navegador:</p>
            <p style='word-break:break-all; color:#666; font-size:12px;'>{enlace}</p>
            <br/>
            <p>— El equipo de Huellitas</p>";

        await EnviarCorreoAsync(destinatario, nombre, "Confirma tu correo en Huellitas", html);
    }

    public async Task EnviarCorreoRecuperacionAsync(string destinatario, string nombre, string token)
    {
        var baseUrl = _config["App:PublicBaseUrl"] ?? _urlCorreoDev;
        var recoveryPath = _config["App:RecoveryPath"] ?? "/api/v1/Usuarios/restablecer";
        var enlace = $"{baseUrl}{recoveryPath}";

        var html = $@"
            <h2>Hola {nombre},</h2>
            <p>Recibimos una solicitud para restablecer tu contraseña en <strong>Huellitas</strong>.</p>
            <p>Tu token de recuperación es:</p>
            <p style='background:#f0f0f0; padding:12px; word-break:break-all; font-family:monospace; font-size:12px;'>
                {token}
            </p>
            <p>Para restablecer tu contraseña, accede aquí:</p>
            <p>
                <a href='{enlace}' style='display:inline-block; padding:12px 24px; background:#2E7D32; color:white; text-decoration:none; border-radius:5px; font-weight:bold;'>
                    Restablecer mi contraseña
                </a>
            </p>
            <p>Este token expira en <strong>30 minutos</strong>.</p>
            <p>Si no solicitaste esto, ignora este mensaje.</p>
            <br/>
            <p>— El equipo de Huellitas</p>";

        await EnviarCorreoAsync(destinatario, nombre, "Recuperación de contraseña - Huellitas", html);
    }

    // ─────────────────────────────────────────────────────
    // Helper privado para no duplicar la lógica de Brevo
    // ─────────────────────────────────────────────────────
    private async Task EnviarCorreoAsync(string destinatario, string nombre, string asunto, string html)
    {
        var apiKey = _config["Brevo:ApiKey"]
            ?? throw new InvalidOperationException("Falta Brevo:ApiKey.");
        var remitenteEmail = _config["Brevo:RemitenteEmail"] ?? "no-reply@huellitas.com";
        var remitenteNombre = _config["Brevo:RemitenteNombre"] ?? "Huellitas";

        BrevoConfig.Default.ApiKey.Clear();
        BrevoConfig.Default.ApiKey.Add("api-key", apiKey);

        var apiInstance = new TransactionalEmailsApi();

        var email = new SendSmtpEmail
        {
            Sender = new SendSmtpEmailSender(remitenteNombre, remitenteEmail),
            To = new List<SendSmtpEmailTo> { new SendSmtpEmailTo(destinatario, nombre) },
            Subject = asunto,
            HtmlContent = html
        };

        await apiInstance.SendTransacEmailAsync(email);
    }
}
