using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Parkini.Entities;
using Parkini.Infrastructure.Data;

namespace Parkini.Infrastructure.Notifications;

public class RappelsJobs
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationService _notificationService;
    private readonly IConfiguration _config;

    private static readonly int[] JoursAvantExpiration = { 30, 15, 7, 1 };
    private static readonly int[] JoursAvantEcheancePaiement = { 7, 3, 1 };

    public RappelsJobs(ApplicationDbContext context, NotificationService notificationService, IConfiguration config)
    {
        _context = context;
        _notificationService = notificationService;
        _config = config;
    }

    public async Task VerifierRenouvellementsAbonnements()
    {
        var abonnementsActifs = await _context.Abonnements
            .Include(a => a.Entreprise)
            .Where(a => a.Statut == StatutAbonnement.Actif)
            .ToListAsync();

        foreach (var abonnement in abonnementsActifs)
        {
            if (abonnement.Entreprise is null) continue;

            var joursRestants = (abonnement.DateFin.Date - DateTime.UtcNow.Date).Days;
            if (!JoursAvantExpiration.Contains(joursRestants)) continue;

            var dejaEnvoyee = await _context.Notifications.AnyAsync(n =>
                n.AbonnementId == abonnement.Id &&
                n.Type == TypeNotification.RenouvellementAbonnement &&
                n.DateCreation.Date == DateTime.UtcNow.Date);

            if (dejaEnvoyee) continue;

            var sujet = $"Votre abonnement expire dans {joursRestants} jour(s)";
            var message =
                $"Bonjour {abonnement.Entreprise.Nom},<br/><br/>" +
                $"Votre abonnement <b>{abonnement.TypeAbonnement}</b> expire le " +
                $"<b>{abonnement.DateFin:dd/MM/yyyy}</b>, soit dans {joursRestants} jour(s).<br/>" +
                $"Merci de le renouveler pour continuer à bénéficier de l'accès au parking.";

            await _notificationService.CreerEtEnvoyerAsync(
                TypeNotification.RenouvellementAbonnement,
                abonnement.Entreprise.Email,
                sujet,
                message,
                entrepriseId: abonnement.EntrepriseId,
                abonnementId: abonnement.Id);
        }
    }

    public async Task VerifierSaturationParkings()
    {
        var seuil = _config.GetValue<double?>("Rappels:SeuilSaturationPourcent") ?? 90;
        var emailAdmin = _config["Rappels:EmailAdministrateur"] ?? "admin@parkini.tn";

        var parkings = await _context.Parkings
            .Include(p => p.Zones)
            .ToListAsync();

        foreach (var parking in parkings)
        {
            var placesTotales = parking.Zones.Sum(z => z.NombrePlaces);
            var placesOccupees = parking.Zones.Sum(z => z.PlacesOccupees);
            if (placesTotales == 0) continue;

            var tauxOccupation = (double)placesOccupees / placesTotales * 100;
            if (tauxOccupation < seuil) continue;

            var dejaAlerteAujourdhui = await _context.Notifications.AnyAsync(n =>
                n.ParkingId == parking.Id &&
                n.Type == TypeNotification.SaturationParking &&
                n.DateCreation.Date == DateTime.UtcNow.Date);

            if (dejaAlerteAujourdhui) continue;

            var sujet = $"Alerte saturation : {parking.Nom}";
            var message =
                $"Le parking <b>{parking.Nom}</b> ({parking.Code}) atteint " +
                $"<b>{tauxOccupation:F0}%</b> d'occupation ({placesOccupees}/{placesTotales} places).";

            await _notificationService.CreerEtEnvoyerAsync(
                TypeNotification.SaturationParking,
                emailAdmin,
                sujet,
                message,
                parkingId: parking.Id);
        }
    }

    public async Task VerifierEcheancesPaiements()
    {
        var paiementsEnAttente = await _context.Paiements
            .Include(p => p.Abonnement)
            .ThenInclude(a => a!.Entreprise)
            .Where(p => p.Statut == StatutPaiement.EnAttente)
            .ToListAsync();

        foreach (var paiement in paiementsEnAttente)
        {
            var entreprise = paiement.Abonnement?.Entreprise;
            if (entreprise is null) continue;

            var joursRestants = (paiement.DateEcheance.Date - DateTime.UtcNow.Date).Days;

            var estRappelAvantEcheance = joursRestants > 0 && JoursAvantEcheancePaiement.Contains(joursRestants);
            var estEnRetard = joursRestants < 0;

            if (!estRappelAvantEcheance && !estEnRetard) continue;

            var dejaEnvoyeeAujourdhui = await _context.Notifications.AnyAsync(n =>
                n.PaiementId == paiement.Id &&
                n.Type == TypeNotification.RappelPaiement &&
                n.DateCreation.Date == DateTime.UtcNow.Date);

            if (dejaEnvoyeeAujourdhui) continue;

            string sujet;
            string message;

            if (estEnRetard)
            {
                var joursRetard = -joursRestants;
                sujet = $"Paiement en retard de {joursRetard} jour(s)";
                message =
                    $"Bonjour {entreprise.Nom},<br/><br/>" +
                    $"Votre paiement de <b>{paiement.Montant:F2} DT</b> (échéance du " +
                    $"<b>{paiement.DateEcheance:dd/MM/yyyy}</b>) est en retard de {joursRetard} jour(s).<br/>" +
                    $"Merci de régulariser au plus vite pour éviter une suspension de votre abonnement.";
            }
            else
            {
                sujet = $"Échéance de paiement dans {joursRestants} jour(s)";
                message =
                    $"Bonjour {entreprise.Nom},<br/><br/>" +
                    $"Un paiement de <b>{paiement.Montant:F2} DT</b> arrive à échéance le " +
                    $"<b>{paiement.DateEcheance:dd/MM/yyyy}</b>, soit dans {joursRestants} jour(s).<br/>" +
                    $"Merci de préparer votre règlement.";
            }

            await _notificationService.CreerEtEnvoyerAsync(
                TypeNotification.RappelPaiement,
                entreprise.Email,
                sujet,
                message,
                entrepriseId: entreprise.Id,
                abonnementId: paiement.AbonnementId,
                paiementId: paiement.Id);
        }
    }

    // A planifier quotidiennement.
    // Rappelle qu'un employe/vehicule/entreprise/parking n'a pas ete modifie depuis
    // N jours (configurable). Anti-spam : pas de nouveau rappel tant que
    // l'enregistrement n'a pas ete modifie depuis le dernier rappel envoye.
    public async Task VerifierMiseAJourInfos()
    {
        var seuilJours = _config.GetValue<int?>("Rappels:JoursAvantRappelMiseAJour") ?? 180;
        var emailAdmin = _config["Rappels:EmailAdministrateur"] ?? "admin@parkini.tn";
        var limite = DateTime.UtcNow.AddDays(-seuilJours);

        // ===== Employes =====
        var employes = await _context.Employes
            .Where(e => e.DateDerniereModification < limite)
            .ToListAsync();

        foreach (var employe in employes)
        {
            var dejaNotifie = await _context.Notifications.AnyAsync(n =>
                n.EmployeId == employe.Id &&
                n.Type == TypeNotification.MiseAJourInfos &&
                n.DateCreation >= employe.DateDerniereModification);

            if (dejaNotifie) continue;

            var joursSansModif = (int)(DateTime.UtcNow - employe.DateDerniereModification).TotalDays;
            var sujet = $"Mise à jour des informations recommandée : {employe.Prenom} {employe.Nom}";
            var message =
                $"Les informations de l'employé <b>{employe.Prenom} {employe.Nom}</b> " +
                $"n'ont pas été modifiées depuis {joursSansModif} jours.<br/>" +
                $"Merci de vérifier qu'elles sont toujours à jour.";

            await _notificationService.CreerEtEnvoyerAsync(
                TypeNotification.MiseAJourInfos,
                employe.Email,
                sujet,
                message,
                employeId: employe.Id);
        }

        // ===== Vehicules =====
        var vehicules = await _context.Vehicules
            .Include(v => v.Employe)
            .Where(v => v.DateDerniereModification < limite)
            .ToListAsync();

        foreach (var vehicule in vehicules)
        {
            var dejaNotifie = await _context.Notifications.AnyAsync(n =>
                n.VehiculeId == vehicule.Id &&
                n.Type == TypeNotification.MiseAJourInfos &&
                n.DateCreation >= vehicule.DateDerniereModification);

            if (dejaNotifie) continue;

            var destinataire = vehicule.Employe?.Email ?? emailAdmin;
            var joursSansModif = (int)(DateTime.UtcNow - vehicule.DateDerniereModification).TotalDays;
            var sujet = $"Mise à jour des informations recommandée : véhicule {vehicule.Immatriculation}";
            var message =
                $"Les informations du véhicule <b>{vehicule.Immatriculation}</b> " +
                $"n'ont pas été modifiées depuis {joursSansModif} jours.<br/>" +
                $"Merci de vérifier qu'elles sont toujours à jour.";

            await _notificationService.CreerEtEnvoyerAsync(
                TypeNotification.MiseAJourInfos,
                destinataire,
                sujet,
                message,
                vehiculeId: vehicule.Id);
        }

        // ===== Entreprises =====
        var entreprises = await _context.Entreprises
            .Where(e => e.DateDerniereModification < limite)
            .ToListAsync();

        foreach (var entreprise in entreprises)
        {
            var dejaNotifie = await _context.Notifications.AnyAsync(n =>
                n.EntrepriseId == entreprise.Id &&
                n.Type == TypeNotification.MiseAJourInfos &&
                n.DateCreation >= entreprise.DateDerniereModification);

            if (dejaNotifie) continue;

            var joursSansModif = (int)(DateTime.UtcNow - entreprise.DateDerniereModification).TotalDays;
            var sujet = $"Mise à jour des informations recommandée : {entreprise.Nom}";
            var message =
                $"Les informations de l'entreprise <b>{entreprise.Nom}</b> " +
                $"n'ont pas été modifiées depuis {joursSansModif} jours.<br/>" +
                $"Merci de vérifier qu'elles sont toujours à jour.";

            await _notificationService.CreerEtEnvoyerAsync(
                TypeNotification.MiseAJourInfos,
                entreprise.Email,
                sujet,
                message,
                entrepriseId: entreprise.Id);
        }

        // ===== Parkings =====
        var parkings = await _context.Parkings
            .Where(p => p.DateDerniereModification < limite)
            .ToListAsync();

        foreach (var parking in parkings)
        {
            var dejaNotifie = await _context.Notifications.AnyAsync(n =>
                n.ParkingId == parking.Id &&
                n.Type == TypeNotification.MiseAJourInfos &&
                n.DateCreation >= parking.DateDerniereModification);

            if (dejaNotifie) continue;

            var joursSansModif = (int)(DateTime.UtcNow - parking.DateDerniereModification).TotalDays;
            var sujet = $"Mise à jour des informations recommandée : parking {parking.Nom}";
            var message =
                $"Les informations du parking <b>{parking.Nom}</b> (code {parking.Code}) " +
                $"n'ont pas été modifiées depuis {joursSansModif} jours.<br/>" +
                $"Merci de vérifier qu'elles sont toujours à jour.";

            await _notificationService.CreerEtEnvoyerAsync(
                TypeNotification.MiseAJourInfos,
                emailAdmin,
                sujet,
                message,
                parkingId: parking.Id);
        }
    }
}