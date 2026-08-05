using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parkini.Application.DTOs;
using Parkini.Infrastructure.Data;

namespace Parkini.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OccupationController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public OccupationController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/occupation?parkingId=1
    [HttpGet]
    public async Task<ActionResult<List<OccupationParkingDto>>> GetOccupation([FromQuery] int? parkingId = null)
    {
        var parkingsQuery = _context.Parkings
            .Include(p => p.Zones)
            .AsQueryable();

        if (parkingId.HasValue)
            parkingsQuery = parkingsQuery.Where(p => p.Id == parkingId.Value);

        var parkings = await parkingsQuery.ToListAsync();

        var dateDebut = DateTime.UtcNow.Date.AddDays(-6);
        var dateFin = DateTime.UtcNow.Date.AddDays(1);

        var accesPeriode = await _context.Acces
            .Where(a => a.DateHeureEntree < dateFin
                     && (a.DateHeureSortie == null || a.DateHeureSortie >= dateDebut))
            .Select(a => new { a.ParkingId, a.DateHeureEntree, a.DateHeureSortie })
            .ToListAsync();

        var resultat = new List<OccupationParkingDto>();

        foreach (var parking in parkings)
        {
            var capacite = parking.Zones.Sum(z => z.NombrePlaces);
            var accesParking = accesPeriode.Where(a => a.ParkingId == parking.Id).ToList();

            var historique = new List<OccupationJourDto>();

            for (var jour = dateDebut; jour < dateFin; jour = jour.AddDays(1))
            {
                var instant = jour.AddHours(12);

                var vehiculesPresents = accesParking.Count(a =>
                    a.DateHeureEntree <= instant &&
                    (a.DateHeureSortie == null || a.DateHeureSortie > instant));

                var taux = capacite > 0
                    ? Math.Round((double)vehiculesPresents / capacite * 100, 1)
                    : 0;

                historique.Add(new OccupationJourDto { Date = jour, TauxOccupation = taux });
            }

            resultat.Add(new OccupationParkingDto
            {
                ParkingId = parking.Id,
                ParkingNom = parking.Nom,
                Capacite = capacite,
                Historique = historique
            });
        }

        return Ok(resultat);
    }
}