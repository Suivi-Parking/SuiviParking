using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parkini.Application.DTOs;
using Parkini.Entities;
using Parkini.Infrastructure.Data;
using Parkini.Infrastructure.Security;

namespace Parkini.Controllers;

// Tableau de bord : agrège en lecture seule les données déjà exposées par les
// autres contrôleurs (Parkings, Paiements, Abonnements, Notifications) pour
// éviter que le frontend n'ait à faire N appels + recalculs cote client.
// Toutes les actions sont en GET : ce contrôleur ne modifie jamais rien.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    // Seuil au dela duquel un parking est considere "en saturation" (alerte).
    private const decimal SeuilSaturationPourcent = 90m;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/dashboard/summary
    // Vue d'ensemble complete : tout ce dont les cartes KPI + la liste de
    // notifications ont besoin, en un seul aller-retour reseau.
    [HasPermission(Permissions.DashboardRead)]
    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary()
    {
        var parkings = await BuildParkingsDto();
        var financier = await BuildFinancierDto();

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
                DateCreation = n.DateCreation,
                DateEnvoi = n.DateEnvoi,
                ErreurDetail = n.ErreurDetail
            })
            .ToListAsync();

        return Ok(new DashboardSummaryDto
        {
            Parkings = parkings,
            Financier = financier,
            DernieresNotifications = dernieresNotifications
        });
    }

    // GET /api/dashboard/occupation-par-parking
    // Alimente le graphique "occupation par parking".
    [HasPermission(Permissions.DashboardRead)]
    [HttpGet("occupation-par-parking")]
    public async Task<ActionResult<List<ParkingOccupationDto>>> GetOccupationParParking()
    {
        var parkings = await _context.Parkings
            .Include(p => p.Zones)
            .Where(p => p.Statut == StatutParking.Actif)
            .ToListAsync();

        var items = parkings
            .Select(ToOccupationDto)
            .OrderByDescending(p => p.TauxOccupation)
            .ToList();

        return Ok(items);
    }

    // GET /api/dashboard/evolution-occupation?periode=jour|semaine&parkingId=5
    // Reconstruit l'occupation reelle a partir des entrees/sorties enregistrees
    // dans la table Acces (nombre de vehicules presents a un instant T =
    // entrees avant T moins sorties avant T). Pas de table de snapshots dediee :
    // on recalcule a la volee, ce qui reste correct meme pour des instants passes.
    [HasPermission(Permissions.DashboardRead)]
    [HttpGet("evolution-occupation")]
    public async Task<ActionResult<List<PointEvolutionOccupationDto>>> GetEvolutionOccupation(
        [FromQuery] string periode = "jour",
        [FromQuery] int? parkingId = null)
    {
        var accesQuery = _context.Acces.Where(a => a.EstAutorise).AsQueryable();
        if (parkingId.HasValue)
            accesQuery = accesQuery.Where(a => a.ParkingId == parkingId.Value);

        var acces = await accesQuery
            .Select(a => new AccesPresence(a.DateHeureEntree, a.DateHeureSortie))
            .ToListAsync();

        var capaciteTotale = await CapaciteTotale(parkingId);
        var maintenant = DateTime.UtcNow;
        var culture = CultureInfo.GetCultureInfo("fr-FR");
        var points = new List<PointEvolutionOccupationDto>();

        if (string.Equals(periode, "semaine", StringComparison.OrdinalIgnoreCase))
        {
            // 7 derniers jours, un point par jour (instant de reference : midi,
            // ou "maintenant" si le jour n'est pas encore arrive a midi).
            for (var i = 6; i >= 0; i--)
            {
                var jour = maintenant.Date.AddDays(-i);
                var instant = jour.AddHours(12) > maintenant ? maintenant : jour.AddHours(12);
                var present = CompterPresents(acces, instant);

                points.Add(new PointEvolutionOccupationDto
                {
                    Label = culture.TextInfo.ToTitleCase(jour.ToString("ddd dd/MM", culture)),
                    Instant = instant,
                    VehiculesPresents = present,
                    TauxOccupation = capaciteTotale > 0 ? Math.Round((decimal)present / capaciteTotale * 100, 1) : 0
                });
            }
        }
        else
        {
            // Vue "jour" : de minuit a l'heure actuelle, un point par heure.
            for (var h = 0; h <= maintenant.Hour; h++)
            {
                var instant = maintenant.Date.AddHours(h);
                var present = CompterPresents(acces, instant);

                points.Add(new PointEvolutionOccupationDto
                {
                    Label = $"{h:00}:00",
                    Instant = instant,
                    VehiculesPresents = present,
                    TauxOccupation = capaciteTotale > 0 ? Math.Round((decimal)present / capaciteTotale * 100, 1) : 0
                });
            }
        }

        return Ok(points);
    }

    // GET /api/dashboard/paiements-par-mois?mois=6
    // Alimente le graphique "paiements par mois" (montant encaisse, mois par mois).
    [HasPermission(Permissions.DashboardRead)]
    [HttpGet("paiements-par-mois")]
    public async Task<ActionResult<List<PaiementsParMoisDto>>> GetPaiementsParMois([FromQuery] int mois = 6)
    {
        mois = Math.Clamp(mois, 1, 24);

        var premierMoisAffiche = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc)
            .AddMonths(-(mois - 1));

        var paiements = await _context.Paiements
            .Where(p => p.Statut == StatutPaiement.Paye && p.DatePaiement != null && p.DatePaiement >= premierMoisAffiche)
            .Select(p => new { p.DatePaiement, p.Montant })
            .ToListAsync();

        var culture = CultureInfo.GetCultureInfo("fr-FR");
        var points = new List<PaiementsParMoisDto>();

        for (var i = 0; i < mois; i++)
        {
            var reference = premierMoisAffiche.AddMonths(i);
            var montant = paiements
                .Where(p => p.DatePaiement!.Value.Year == reference.Year && p.DatePaiement.Value.Month == reference.Month)
                .Sum(p => p.Montant);

            points.Add(new PaiementsParMoisDto
            {
                Mois = culture.TextInfo.ToTitleCase(reference.ToString("MMM yyyy", culture)),
                Annee = reference.Year,
                MoisNumero = reference.Month,
                MontantPercu = montant
            });
        }

        return Ok(points);
    }

    // GET /api/dashboard/revenus?dateDebut=2026-01-01&dateFin=2026-08-01&entrepriseId=3
    // Story dediee : "En tant qu'Administrateur, je veux consulter les revenus
    // totaux et par entreprise afin de piloter la performance financiere."
    // Rapport complet (total + repartition + % par entreprise), filtrable par
    // periode et par entreprise, pour la page "Revenus" du menu Gestion.
    [HasPermission(Permissions.DashboardRead)]
    [HttpGet("revenus")]
    public async Task<ActionResult<RevenusRapportDto>> GetRevenus(
        [FromQuery] DateTime? dateDebut = null,
        [FromQuery] DateTime? dateFin = null,
        [FromQuery] int? entrepriseId = null)
    {
        if (dateDebut.HasValue && dateFin.HasValue && dateDebut > dateFin)
            return BadRequest(new { message = "La date de début doit précéder la date de fin." });

        var parEntreprise = await BuildRevenusParEntreprise(dateDebut, dateFin, entrepriseId);
        var revenuTotal = parEntreprise.Sum(r => r.MontantTotal);

        foreach (var ligne in parEntreprise)
            ligne.PourcentageDuTotal = revenuTotal > 0 ? Math.Round(ligne.MontantTotal / revenuTotal * 100, 1) : 0;

        return Ok(new RevenusRapportDto
        {
            RevenuTotal = revenuTotal,
            NombrePaiements = parEntreprise.Sum(r => r.NombrePaiements),
            DateDebut = dateDebut,
            DateFin = dateFin,
            ParEntreprise = parEntreprise
        });
    }

    // GET /api/dashboard/revenus-par-entreprise
    // Story dediee : "En tant qu'Administrateur, je veux consulter les revenus
    // totaux et par entreprise afin de piloter la performance financiere."
    [HasPermission(Permissions.DashboardRead)]
    [HttpGet("revenus-par-entreprise")]
    public async Task<ActionResult<List<RevenuEntrepriseDto>>> GetRevenusParEntreprise()
    {
        return Ok(await BuildRevenusParEntreprise());
    }

    // ===== Aides internes =====

    private async Task<DashboardParkingsDto> BuildParkingsDto()
    {
        var parkings = await _context.Parkings
            .Include(p => p.Zones)
            .Where(p => p.Statut == StatutParking.Actif)
            .ToListAsync();

        var occupations = parkings.Select(ToOccupationDto).ToList();
        var avecCapacite = occupations.Where(p => p.CapaciteMax > 0).ToList();

        var capaciteTotale = occupations.Sum(p => p.CapaciteMax);
        var placesOccupeesTotal = occupations.Sum(p => p.PlacesOccupees);
        var tauxGlobal = capaciteTotale > 0
            ? Math.Round((decimal)placesOccupeesTotal / capaciteTotale * 100, 1)
            : 0;

        var alertes = avecCapacite
            .Where(p => p.TauxOccupation >= SeuilSaturationPourcent)
            .OrderByDescending(p => p.TauxOccupation)
            .Select(p => new AlerteSaturationDto
            {
                ParkingId = p.Id,
                ParkingNom = p.Nom,
                TauxOccupation = p.TauxOccupation,
                PlacesLibres = p.CapaciteMax - p.PlacesOccupees
            })
            .ToList();

        return new DashboardParkingsDto
        {
            NombreTotalParkings = parkings.Count,
            TauxOccupationGlobal = tauxGlobal,
            ParkingLePlusOccupe = avecCapacite.OrderByDescending(p => p.TauxOccupation).FirstOrDefault(),
            ParkingLeMoinsOccupe = avecCapacite.OrderBy(p => p.TauxOccupation).FirstOrDefault(),
            AlertesSaturation = alertes
        };
    }

    private async Task<DashboardFinancierDto> BuildFinancierDto()
    {
        var paiements = await _context.Paiements.ToListAsync();
        var maintenant = DateTime.UtcNow;
        var debutMois = new DateTime(maintenant.Year, maintenant.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var montantPercuMoisEnCours = paiements
            .Where(p => p.Statut == StatutPaiement.Paye && p.DatePaiement >= debutMois)
            .Sum(p => p.Montant);

        var montantPercuCumule = paiements
            .Where(p => p.Statut == StatutPaiement.Paye)
            .Sum(p => p.Montant);

        var montantImpaye = paiements
            .Where(p => StatutEffectifPaiement(p) is "EnAttente" or "EnRetard")
            .Sum(p => p.Montant);

        var nombreEnRetard = paiements.Count(p => StatutEffectifPaiement(p) == "EnRetard");

        var abonnements = await _context.Abonnements.ToListAsync();
        var dans30Jours = maintenant.AddDays(30);
        var nombreExpirantSous30Jours = abonnements.Count(a =>
            a.Statut == StatutAbonnement.Actif && a.DateFin >= maintenant && a.DateFin <= dans30Jours);

        var repartition = abonnements
            .Where(a => a.Statut == StatutAbonnement.Actif)
            .GroupBy(a => string.IsNullOrWhiteSpace(a.TypeAbonnement) ? "Non precise" : a.TypeAbonnement)
            .Select(g => new RepartitionAbonnementTypeDto { Type = g.Key, Nombre = g.Count() })
            .OrderByDescending(r => r.Nombre)
            .ToList();

        var revenusParEntreprise = await BuildRevenusParEntreprise();

        return new DashboardFinancierDto
        {
            MontantPercuMoisEnCours = montantPercuMoisEnCours,
            MontantPercuCumule = montantPercuCumule,
            MontantImpayeTotal = montantImpaye,
            NombrePaiementsEnRetard = nombreEnRetard,
            NombreAbonnementsExpirantSous30Jours = nombreExpirantSous30Jours,
            RepartitionAbonnements = repartition,
            RevenusParEntreprise = revenusParEntreprise,
            RevenuTotalToutesEntreprises = revenusParEntreprise.Sum(r => r.MontantTotal)
        };
    }

    private async Task<List<RevenuEntrepriseDto>> BuildRevenusParEntreprise(
        DateTime? dateDebut = null, DateTime? dateFin = null, int? entrepriseId = null)
    {
        var query = _context.Paiements
            .Include(p => p.Abonnement).ThenInclude(a => a!.Entreprise)
            .Where(p => p.Statut == StatutPaiement.Paye)
            .AsQueryable();

        if (dateDebut.HasValue)
            query = query.Where(p => p.DatePaiement >= dateDebut.Value);
        if (dateFin.HasValue)
            query = query.Where(p => p.DatePaiement <= dateFin.Value);
        if (entrepriseId.HasValue)
            query = query.Where(p => p.Abonnement != null && p.Abonnement.Entreprise != null && p.Abonnement.Entreprise.Id == entrepriseId.Value);

        var paiements = await query.ToListAsync();

        return paiements
            .Where(p => p.Abonnement?.Entreprise != null)
            .GroupBy(p => new { p.Abonnement!.Entreprise!.Id, p.Abonnement.Entreprise.Nom })
            .Select(g => new RevenuEntrepriseDto
            {
                EntrepriseId = g.Key.Id,
                EntrepriseNom = g.Key.Nom,
                MontantTotal = g.Sum(p => p.Montant),
                NombrePaiements = g.Count()
            })
            .OrderByDescending(r => r.MontantTotal)
            .ToList();
    }

    private async Task<int> CapaciteTotale(int? parkingId)
    {
        var query = _context.Parkings.Include(p => p.Zones).AsQueryable();
        if (parkingId.HasValue)
            query = query.Where(p => p.Id == parkingId.Value);

        var parkings = await query.ToListAsync();
        return parkings.Sum(p => p.Zones.Sum(z => z.NombrePlaces));
    }

    // Projection minimale utilisee pour reconstruire l'occupation a un instant T
    // (evite de charger l'entite Acces complete juste pour 2 champs).
    private record AccesPresence(DateTime DateHeureEntree, DateTime? DateHeureSortie);

    // Nombre de vehicules encore "presents" (entres avant l'instant, pas encore sortis) a un instant T.
    private static int CompterPresents(IEnumerable<AccesPresence> acces, DateTime instant) =>
        acces.Count(a => a.DateHeureEntree <= instant && (a.DateHeureSortie == null || a.DateHeureSortie > instant));

    // Duplique volontairement CalculerStatutEffectif de PaiementsController :
    // meme logique metier (un paiement "EnAttente" dont l'echeance est depassee
    // est "EnRetard"), pas de dependance croisee entre controleurs.
    private static string StatutEffectifPaiement(Paiement p)
    {
        if (p.Statut == StatutPaiement.EnAttente && p.DateEcheance < DateTime.UtcNow)
            return "EnRetard";

        return p.Statut.ToString();
    }

    private static ParkingOccupationDto ToOccupationDto(Parking p)
    {
        var capaciteMax = p.Zones.Sum(z => z.NombrePlaces);
        var placesOccupees = p.Zones.Sum(z => z.PlacesOccupees);

        return new ParkingOccupationDto
        {
            Id = p.Id,
            Nom = p.Nom,
            CapaciteMax = capaciteMax,
            PlacesOccupees = placesOccupees,
            TauxOccupation = capaciteMax == 0 ? 0 : Math.Round((decimal)placesOccupees / capaciteMax * 100, 1)
        };
    }
}
