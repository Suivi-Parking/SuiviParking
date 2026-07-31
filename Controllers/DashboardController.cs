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

        var totalEmployes = await _context.Employes.CountAsync();
        var totalVehicules = await _context.Vehicules.CountAsync();

        var totalParkings = await _context.Parkings.CountAsync();

        var zones = await _context.Zones.ToListAsync();
        var totalPlaces = zones.Sum(z => z.NombrePlaces);
        var placesOccupees = zones.Sum(z => z.PlacesOccupees);
        var tauxOccupation = totalPlaces > 0
            ? Math.Round((double)placesOccupees / totalPlaces * 100, 1)
            : 0;

        var paiements = await _context.Paiements.ToListAsync();

        var revenuTotal = paiements
            .Where(p => p.Statut == StatutPaiement.Paye)
            .Sum(p => p.Montant);

        var debutMois = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var revenuMensuel = paiements
            .Where(p => p.Statut == StatutPaiement.Paye && p.DatePaiement.HasValue && p.DatePaiement.Value >= debutMois)
            .Sum(p => p.Montant);

        var montantImpaye = paiements
            .Where(p => p.Statut == StatutPaiement.EnAttente)
            .Sum(p => p.Montant);

        return Ok(new DashboardDto
        {
            TotalEntreprises = entreprises.Count,
            EntreprisesActives = entreprisesActives,
            EntreprisesInactives = entreprisesInactives,
            AbonnementsActifs = abonnementsActifs,
            AbonnementsExpires = abonnementsExpires,
            TotalEmployes = totalEmployes,
            TotalVehicules = totalVehicules,
            TotalParkings = totalParkings,
            TauxOccupationGlobal = tauxOccupation,
            RevenuTotal = revenuTotal,
            RevenuMensuel = revenuMensuel,
            MontantImpaye = montantImpaye
        });
    }

    private static string CalculerStatutEffectif(Abonnement a)
    {
        if (a.Statut == StatutAbonnement.Actif && a.DateFin < DateTime.UtcNow)
            return "Expire";
        return a.Statut.ToString();
    }
}