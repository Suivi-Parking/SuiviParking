using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Parkini.Infrastructure.Notifications;

public class MailKitEmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public MailKitEmailSender(IConfiguration config)
    {
        _config = config;
    }

    public async Task EnvoyerAsync(string destinataire, string sujet, string corpsHtml)
    {
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(_config["Smtp:ExpediteurEmail"]));
        message.To.Add(MailboxAddress.Parse(destinataire));
        message.Subject = sujet;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = corpsHtml };

        using var client = new SmtpClient();

        var port = int.Parse(_config["Smtp:Port"] ?? "587");
        await client.ConnectAsync(_config["Smtp:Host"], port, SecureSocketOptions.StartTls);

        var utilisateur = _config["Smtp:Utilisateur"];
        if (!string.IsNullOrEmpty(utilisateur))
            await client.AuthenticateAsync(utilisateur, _config["Smtp:MotDePasse"]);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
