using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parkini.Application.DTOs;
using Parkini.Entities;
using Parkini.Infrastructure.Calibration;
using Parkini.Infrastructure.Data;

namespace Parkini.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CalibrationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IImageParkingService _imageService;

    public CalibrationController(ApplicationDbContext context, IImageParkingService imageService)
    {
        _context = context;
        _imageService = imageService;
    }

    // GET api/Calibration/image/5
    // Sert l'image du parking pour que l'UI admin l'affiche en fond du canvas.
    [HttpGet("image/{parkingId:int}")]
    public async Task<IActionResult> GetImage(int parkingId)
    {
        try
        {
            var (bytes, mimeType) = await _imageService.RecupererImageAsync(parkingId);
            return File(bytes, mimeType);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // GET api/Calibration/5
    // Renvoie les zones deja calibrees pour ce parking (liste vide si aucune).
    [HttpGet("{parkingId:int}")]
    public async Task<ActionResult<List<CalibrationZoneDto>>> GetZones(int parkingId)
    {
        var zones = await _context.ZonesCalibration
            .Include(z => z.Zone)
            .Where(z => z.ParkingId == parkingId)
            .OrderBy(z => z.Id)
            .ToListAsync();

        return Ok(zones.Select(ToDto).ToList());
    }

    // PUT api/Calibration/5
    // Remplace l'integralite de la calibration d'un parking en une fois.
    [HttpPut("{parkingId:int}")]
    public async Task<ActionResult<List<CalibrationZoneDto>>> SauvegarderZones(
        int parkingId, List<CalibrationZoneInputDto> zones)
    {
        var zonesDuParking = await _context.Zones
            .Where(z => z.ParkingId == parkingId)
            .ToDictionaryAsync(z => z.Id);

        if (zonesDuParking.Count == 0)
            return NotFound(new { message = $"Parking {parkingId} introuvable ou sans zone." });

        foreach (var zone in zones)
        {
            if (!zonesDuParking.ContainsKey(zone.ZoneId))
                return BadRequest(new { message = $"La zone {zone.ZoneId} n'appartient pas au parking {parkingId}." });

            if (zone.Points.Count < 3)
                return BadRequest(new { message = $"La zone \"{zonesDuParking[zone.ZoneId].Nom}\" doit avoir au moins 3 points (polygone)." });

            if (zone.Points.Any(p => p.X < 0 || p.X > 1 || p.Y < 0 || p.Y > 1))
                return BadRequest(new { message = $"Les points de la zone \"{zonesDuParking[zone.ZoneId].Nom}\" doivent etre normalises entre 0 et 1." });
        }

        var doublons = zones.GroupBy(z => z.ZoneId).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (doublons.Count > 0)
            return BadRequest(new { message = "Chaque zone physique ne peut avoir qu'un seul polygone de calibration." });

        var anciennesZones = await _context.ZonesCalibration
            .Where(z => z.ParkingId == parkingId)
            .ToListAsync();
        _context.ZonesCalibration.RemoveRange(anciennesZones);

        var nouvellesZones = zones.Select(z => new ZoneCalibration
        {
            ParkingId = parkingId,
            ZoneId = z.ZoneId,
            Points = z.Points.Select(p => new PointCalibration { X = p.X, Y = p.Y }).ToList(),
            DateCreation = DateTime.UtcNow,
            DateDerniereModification = DateTime.UtcNow
        }).ToList();

        _context.ZonesCalibration.AddRange(nouvellesZones);
        await _context.SaveChangesAsync();

        // Recharge avec la Zone incluse pour renvoyer NomZone correctement rempli
        foreach (var z in nouvellesZones)
            z.Zone = zonesDuParking[z.ZoneId];

        return Ok(nouvellesZones.Select(ToDto).ToList());
    }

    private static CalibrationZoneDto ToDto(ZoneCalibration z) => new()
    {
        Id = z.Id,
        ParkingId = z.ParkingId,
        ZoneId = z.ZoneId,
        NomZone = z.Zone?.Nom ?? string.Empty,
        Points = z.Points.Select(p => new PointDto { X = p.X, Y = p.Y }).ToList(),
        DateCreation = z.DateCreation,
        DateDerniereModification = z.DateDerniereModification
    };
}
