using Microsoft.Extensions.Logging;
using Parkini.Entities;
using Parkini.Infrastructure.Data;

namespace Parkini.Infrastructure.Notifications;

public class NotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ApplicationDbContext context, IEmailSender emailSender, ILogger<NotificationService> logger)
    {
        _context = context;
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<Notification> CreerEtEnvoyerAsync(
        TypeNotification type,
        string destinataire,
        string sujet,
        string message,
        int? entrepriseId = null,
        int? abonnementId = null,
        int? parkingId = null,
        int? accesId = null,
        int? paiementId = null,
        int? employeId = null,
        int? vehiculeId = null,
        CanalNotification canal = CanalNotification.Email)
    {
        var notification = new Notification
        {
            Type = type,
            Canal = canal,
            Destinataire = destinataire,
            Sujet = sujet,
            Message = message,
            EntrepriseId = entrepriseId,
            AbonnementId = abonnementId,
            ParkingId = parkingId,
            AccesId = accesId,
            PaiementId = paiementId,
            EmployeId = employeId,
            VehiculeId = vehiculeId
        };

        try
        {
            if (canal == CanalNotification.Email)
            {
                await _emailSender.EnvoyerAsync(destinataire, sujet, message);
            }

            notification.Statut = StatutNotification.Envoyee;
            notification.DateEnvoi = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            notification.Statut = StatutNotification.Echouee;
            notification.ErreurDetail = ex.Message;
            _logger.LogError(ex, "Echec envoi notification {Type} vers {Destinataire}", type, destinataire);
        }

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return notification;
    }
}