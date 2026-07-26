namespace Parkini.Infrastructure.Notifications;

public interface IEmailSender
{
    Task EnvoyerAsync(string destinataire, string sujet, string corpsHtml);
}
