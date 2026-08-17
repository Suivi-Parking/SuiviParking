using Microsoft.EntityFrameworkCore;
using Parkini.Application.DTOs;
using Parkini.Infrastructure.Data;

namespace Parkini.Infrastructure.Vlm;

public interface IOccupationAnalyseService
{
    /// <summary>
    /// Analyse l'image d'un parking (avec ses zones calibrees si disponibles)
    /// et met a jour Zone.PlacesOccupees en consequence. Leve une exception si
    /// l'analyse echoue (image inaccessible, VLM injoignable, etc.) -- a
    /// l'appelant de decider s'il continue ou s'arrete (voir OccupationJobs).
    /// </summary>
    Task<ResultatAnalyseVlm> AnalyserParkingAsync(int parkingId, string? imageUrlOverride = null, CancellationToken ct = default);
}

public class OccupationAnalyseService : IOccupationAnalyseService
{
    private readonly IVlmOccupationService _vlmService;
    private readonly ApplicationDbContext _context;

    private const string ImageDemoParDefaut =
        "https://images.pexels.com/photos/63294/autos-technology-vw-multi-storey-car-park-63294.jpeg";

    public OccupationAnalyseService(IVlmOccupationService vlmService, ApplicationDbContext context)
    {
        _vlmService = vlmService;
        _context = context;
    }

    public async Task<ResultatAnalyseVlm> AnalyserParkingAsync(int parkingId, string? imageUrlOverride = null, CancellationToken ct = default)
    {
        var parking = await _context.Parkings
            .Include(p => p.Zones)
            .FirstOrDefaultAsync(p => p.Id == parkingId, ct);

        if (parking is null)
            throw new InvalidOperationException($"Parking {parkingId} introuvable.");

        var calibrations = await _context.ZonesCalibration
            .Where(z => z.ParkingId == parkingId)
            .ToListAsync(ct);

        var requete = new AnalyseOccupationRequest
        {
            ParkingId = parkingId,
            ImageUrl = imageUrlOverride ?? parking.ImageUrl ?? ImageDemoParDefaut,
            Zones = calibrations.Count > 0
                ? calibrations.Select(c => new ZoneCalibrationDto
                {
                    Nom = $"zone_{c.ZoneId}",
                    Points = c.Points.Select(p => new PointDto { X = p.X, Y = p.Y }).ToList()
                }).ToList()
                : null
        };

        var resultat = await _vlmService.AnalyserAsync(requete, ct);

        if (calibrations.Count > 0)
        {
            foreach (var zoneVlm in resultat.Zones)
            {
                if (!zoneVlm.NomZone.StartsWith("zone_") ||
                    !int.TryParse(zoneVlm.NomZone.AsSpan(5), out var zoneId))
                {
                    continue;
                }

                var zonePhysique = parking.Zones.FirstOrDefault(z => z.Id == zoneId)
                    ?? await _context.Zones.FindAsync(new object[] { zoneId }, ct);

                if (zonePhysique is null)
                    continue;

                zonePhysique.PlacesOccupees = Math.Clamp(zoneVlm.VehiculesDetectes, 0, zonePhysique.NombrePlaces);
            }
        }
        else
        {
            var premiereZone = parking.Zones.FirstOrDefault();
            if (premiereZone is not null && resultat.Zones.Count > 0)
            {
                premiereZone.PlacesOccupees = Math.Clamp(
                    resultat.Zones[0].VehiculesDetectes, 0, premiereZone.NombrePlaces);
            }
        }

        parking.DateDerniereModification = DateTime.UtcNow;
        parking.DateDerniereAnalyseIa = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);

        return resultat;
    }
}