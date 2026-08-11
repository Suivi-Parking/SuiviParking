using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parkini.Application.DTOs;
using Parkini.Entities;
using Parkini.Infrastructure.Data;

namespace Parkini.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    // Seuils de couleur pour le taux d'occupation (ticket "graphique d'occupation")
    private const double SeuilJaune = 60;
    private const double SeuilRouge = 85;
    private const double SeuilCritique = 90;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDto>> GetDashboard()
    {
        var entreprises = await _context.Entreprises.IgnoreQueryFilters().ToListAsync();
        var entreprisesActives = entreprises.Count(e => !e.EstSupprime);
        var entreprisesInactives = entreprises.Count(e => e.EstSupprime);

        var abonnements = await _context.Abonnements.IgnoreQueryFilters().ToListAsync();
        var abonnementsActifs = abonnements.Count(a => CalculerStatutEffectif(a) == "Actif");
        var abonnementsExpires = abonnements.Count(a => CalculerStatutEffectif(a) == "Expire");

        var dans30Jours = DateTime.UtcNow.AddDays(30);
        var abonnementsExpirantSous30Jours = abonnements.Count(a =>
            CalculerStatutEffectif(a) == "Actif" && a.DateFin >= DateTime.UtcNow && a.DateFin <= dans30Jours);

        var repartitionAbonnements = abonnements
            .Where(a => CalculerStatutEffectif(a) == "Actif")
            .GroupBy(a => string.IsNullOrWhiteSpace(a.TypeAbonnement) ? "Non renseigné" : a.TypeAbonnement)
            .Select(g => new RepartitionAbonnementDto { Type = g.Key, Nombre = g.Count() })
            .OrderByDescending(r => r.Nombre)
            .ToList();

        var totalEmployes = await _context.Employes.CountAsync();
        var totalVehicules = await _context.Vehicules.CountAsync();

        var totalParkings = await _context.Parkings.CountAsync();

        // ===== Occupation par parking =====
        var parkings = await _context.Parkings.ToListAsync();
        var zones = await _context.Zones.Where(z => z.EstActive).ToListAsync();

        var occupationParParking = parkings.Select(p =>
        {
            var zonesDuParking = zones.Where(z => z.ParkingId == p.Id).ToList();
            var places = zonesDuParking.Sum(z => z.NombrePlaces);
            var occupees = zonesDuParking.Sum(z => z.PlacesOccupees);
            var taux = places > 0 ? Math.Round((double)occupees / places * 100, 1) : 0;
            return new ParkingOccupationDto
            {
                ParkingId = p.Id,
                Nom = p.Nom,
                NombrePlaces = places,
                PlacesOccupees = occupees,
                TauxOccupation = taux,
                Statut = StatutCouleur(taux)
            };
        }).ToList();

        var totalPlaces = zones.Sum(z => z.NombrePlaces);
        var placesOccupees = zones.Sum(z => z.PlacesOccupees);
        var tauxOccupation = totalPlaces > 0
            ? Math.Round((double)placesOccupees / totalPlaces * 100, 1)
            : 0;

        var parkingsAvecPlaces = occupationParParking.Where(o => o.NombrePlaces > 0).ToList();
        var parkingLePlusOccupe = parkingsAvecPlaces
            .OrderByDescending(o => o.TauxOccupation)
            .Select(o => new ParkingResumeDto { ParkingId = o.ParkingId, Nom = o.Nom, TauxOccupation = o.TauxOccupation })
            .FirstOrDefault();
        var parkingLeMoinsOccupe = parkingsAvecPlaces
            .OrderBy(o => o.TauxOccupation)
            .Select(o => new ParkingResumeDto { ParkingId = o.ParkingId, Nom = o.Nom, TauxOccupation = o.TauxOccupation })
            .FirstOrDefault();

        var alertesSaturation = occupationParParking
            .Where(o => o.TauxOccupation >= SeuilRouge)
            .Select(o => new AlerteSaturationDto
            {
                ParkingId = o.ParkingId,
                NomParking = o.Nom,
                TauxOccupation = o.TauxOccupation,
                Niveau = o.TauxOccupation >= SeuilCritique ? "critique" : "eleve"
            })
            .OrderByDescending(a => a.TauxOccupation)
            .ToList();

        // ===== Paiements / finances =====
        var paiements = await _context.Paiements.ToListAsync();
        var debutMois = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var revenuTotal = paiements
            .Where(p => p.Statut == StatutPaiement.Paye)
            .Sum(p => p.Montant);

        var revenuMensuel = paiements
            .Where(p => p.Statut == StatutPaiement.Paye && p.DatePaiement.HasValue && p.DatePaiement.Value >= debutMois)
            .Sum(p => p.Montant);

        // Impaye = pas encore paye, qu'il soit en attente ou deja constate en retard.
        var montantImpaye = paiements
            .Where(p => p.Statut == StatutPaiement.EnAttente || p.Statut == StatutPaiement.EnRetard)
            .Sum(p => p.Montant);

        var nombrePaiementsEnRetard = paiements.Count(p => p.Statut == StatutPaiement.EnRetard);

        var revenuParEntreprise = CalculerRevenuParEntreprise(paiements, abonnements, entreprises, debutMois);

        var revenusParMois = Enumerable.Range(0, 6)
            .Select(i => DateTime.UtcNow.AddMonths(-i))
            .Select(d => new { d.Year, d.Month })
            .Reverse()
            .Select(ym =>
            {
                var debut = new DateTime(ym.Year, ym.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var fin = debut.AddMonths(1);
                var montant = paiements
                    .Where(p => p.Statut == StatutPaiement.Paye && p.DatePaiement.HasValue
                                && p.DatePaiement.Value >= debut && p.DatePaiement.Value < fin)
                    .Sum(p => p.Montant);
                return new RevenuMensuelDto
                {
                    Annee = ym.Year,
                    Mois = ym.Month,
                    NomMois = debut.ToString("MMM", new System.Globalization.CultureInfo("fr-FR")),
                    Montant = montant
                };
            })
            .ToList();

        // ===== Dernieres notifications =====
        var dernieresNotifications = await _context.Notifications
            .Include(n => n.Entreprise)
            .OrderByDescending(n => n.DateCreation)
            .Take(5)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type.ToString(),
                Canal = n.Canal.ToString(),
                Statut = n.Statut.ToString(),
                Destinataire = n.Destinataire,
                Sujet = n.Sujet,
                Message = n.Message,
                EntrepriseId = n.EntrepriseId,
                EntrepriseNom = n.Entreprise != null ? n.Entreprise.Nom : null,
                AbonnementId = n.AbonnementId,
                ParkingId = n.ParkingId,
                AccesId = n.AccesId,
                PaiementId = n.PaiementId,
                EmployeId = n.EmployeId,
                VehiculeId = n.VehiculeId,
                DateCreation = n.DateCreation,
                DateEnvoi = n.DateEnvoi,
                ErreurDetail = n.ErreurDetail
            })
            .ToListAsync();

        return Ok(new DashboardDto
        {
            TotalEntreprises = entreprises.Count,
            EntreprisesActives = entreprisesActives,
            EntreprisesInactives = entreprisesInactives,
            AbonnementsActifs = abonnementsActifs,
            AbonnementsExpires = abonnementsExpires,
            AbonnementsExpirantSous30Jours = abonnementsExpirantSous30Jours,
            RepartitionAbonnements = repartitionAbonnements,
            TotalEmployes = totalEmployes,
            TotalVehicules = totalVehicules,
            TotalParkings = totalParkings,
            TauxOccupationGlobal = tauxOccupation,
            ParkingLePlusOccupe = parkingLePlusOccupe,
            ParkingLeMoinsOccupe = parkingLeMoinsOccupe,
            OccupationParParking = occupationParParking,
            AlertesSaturation = alertesSaturation,
            RevenuTotal = revenuTotal,
            RevenuMensuel = revenuMensuel,
            MontantImpaye = montantImpaye,
            NombrePaiementsEnRetard = nombrePaiementsEnRetard,
            RevenuParEntreprise = revenuParEntreprise,
            RevenusParMois = revenusParMois,
            DernieresNotifications = dernieresNotifications
        });
    }

    /// <summary>Endpoint dédié ticket "revenus totaux et par entreprise" (réutilisable sans charger tout le dashboard).</summary>
    [HttpGet("revenus-entreprises")]
    public async Task<ActionResult<List<RevenuEntrepriseDto>>> GetRevenusParEntreprise()
    {
        var entreprises = await _context.Entreprises.IgnoreQueryFilters().ToListAsync();
        var abonnements = await _context.Abonnements.IgnoreQueryFilters().ToListAsync();
        var paiements = await _context.Paiements.ToListAsync();
        var debutMois = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var result = CalculerRevenuParEntreprise(paiements, abonnements, entreprises, debutMois);
        return Ok(result);
    }

    /// <summary>
    /// Paiement n'a pas d'EntrepriseId direct : le lien passe par Paiement.AbonnementId -> Abonnement.EntrepriseId.
    /// </summary>
    private static List<RevenuEntrepriseDto> CalculerRevenuParEntreprise(
        List<Paiement> paiements, List<Abonnement> abonnements, List<Entreprise> entreprises, DateTime debutMois)
    {
        var entrepriseIdParAbonnement = abonnements.ToDictionary(a => a.Id, a => a.EntrepriseId);

        return paiements
            .Where(p => entrepriseIdParAbonnement.ContainsKey(p.AbonnementId))
            .GroupBy(p => entrepriseIdParAbonnement[p.AbonnementId])
            .Select(g =>
            {
                var entreprise = entreprises.FirstOrDefault(e => e.Id == g.Key);
                return new RevenuEntrepriseDto
                {
                    EntrepriseId = g.Key,
                    NomEntreprise = entreprise?.Nom ?? "Entreprise inconnue",
                    RevenuTotal = g.Where(p => p.Statut == StatutPaiement.Paye).Sum(p => p.Montant),
                    RevenuMensuel = g.Where(p => p.Statut == StatutPaiement.Paye && p.DatePaiement.HasValue && p.DatePaiement.Value >= debutMois)
                        .Sum(p => p.Montant),
                    MontantImpaye = g.Where(p => p.Statut == StatutPaiement.EnAttente || p.Statut == StatutPaiement.EnRetard)
                        .Sum(p => p.Montant)
                };
            })
            .Where(r => r.RevenuTotal > 0 || r.MontantImpaye > 0)
            .OrderByDescending(r => r.RevenuTotal)
            .ToList();
    }

    private static string StatutCouleur(double taux)
    {
        if (taux > SeuilRouge) return "rouge";
        if (taux >= SeuilJaune) return "jaune";
        return "vert";
    }

    private static string CalculerStatutEffectif(Abonnement a)
    {
        if (a.Statut == StatutAbonnement.Actif && a.DateFin < DateTime.UtcNow)
            return "Expire";
        return a.Statut.ToString();
    }
}