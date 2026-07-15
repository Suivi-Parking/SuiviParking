using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parkini.Application.DTOs;
using Parkini.Entities;
using Parkini.Infrastructure.Data;

namespace Parkini.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParkingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ParkingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/parkings?page=1&pageSize=10&search=xxx
    // Recherche par nom, code ou adresse (critere de consultation du ticket)
    [HttpGet]
    public async Task<ActionResult<ParkingListeResponseDto>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var query = _context.Parkings.Include(p => p.Zones).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Nom.Contains(search) ||
                p.Code.Contains(search) ||
                p.Adresse.Contains(search));
        }

        var totalCount = await query.CountAsync();

        var parkings = await query
            .OrderBy(p => p.Nom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = parkings.Select(p => ToDto(p, inclureZones: false)).ToList();

        return Ok(new ParkingListeResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    // GET /api/parkings/5 (avec le detail des zones et le taux d'occupation)
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ParkingDto>> GetById(int id)
    {
        var parking = await _context.Parkings
            .Include(p => p.Zones)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (parking is null)
            return NotFound();

        return Ok(ToDto(parking, inclureZones: true));
    }

    // POST /api/parkings
    // Regles : code unique, au moins une zone obligatoire
    [HttpPost]
    public async Task<ActionResult<ParkingDto>> Create(CreateParkingDto dto)
    {
        if (dto.Zones is null || dto.Zones.Count == 0)
            return BadRequest("Un parking doit avoir au moins une zone.");

        var codeExiste = await _context.Parkings.AnyAsync(p => p.Code == dto.Code);
        if (codeExiste)
            return Conflict($"Le code parking '{dto.Code}' est deja utilise.");

        if (!dto.Ouvert24h24 && (dto.HeureOuverture is null || dto.HeureFermeture is null))
            return BadRequest("Les horaires d'ouverture/fermeture sont requis si le parking n'est pas ouvert 24h/24.");

        var parking = new Parking
        {
            Nom = dto.Nom,
            Code = dto.Code,
            Adresse = dto.Adresse,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Ouvert24h24 = dto.Ouvert24h24,
            HeureOuverture = dto.Ouvert24h24 ? null : dto.HeureOuverture,
            HeureFermeture = dto.Ouvert24h24 ? null : dto.HeureFermeture,
            ImageUrl = dto.ImageUrl,
            Statut = ParseStatut(dto.Statut) ?? StatutParking.Actif
        };

        foreach (var zoneDto in dto.Zones)
        {
            parking.Zones.Add(new Zone
            {
                Nom = zoneDto.Nom,
                Type = zoneDto.Type,
                NombrePlaces = zoneDto.NombrePlaces
            });
        }

        _context.Parkings.Add(parking);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = parking.Id }, ToDto(parking, inclureZones: true));
    }

    // PUT /api/parkings/5 (infos generales uniquement, pas les zones)
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateParkingDto dto)
    {
        var parking = await _context.Parkings.FindAsync(id);
        if (parking is null)
            return NotFound();

        var codeUtiliseParUnAutre = await _context.Parkings
            .AnyAsync(p => p.Code == dto.Code && p.Id != id);
        if (codeUtiliseParUnAutre)
            return Conflict($"Le code parking '{dto.Code}' est deja utilise par un autre parking.");

        if (!dto.Ouvert24h24 && (dto.HeureOuverture is null || dto.HeureFermeture is null))
            return BadRequest("Les horaires d'ouverture/fermeture sont requis si le parking n'est pas ouvert 24h/24.");

        parking.Nom = dto.Nom;
        parking.Code = dto.Code;
        parking.Adresse = dto.Adresse;
        parking.Latitude = dto.Latitude;
        parking.Longitude = dto.Longitude;
        parking.Ouvert24h24 = dto.Ouvert24h24;
        parking.HeureOuverture = dto.Ouvert24h24 ? null : dto.HeureOuverture;
        parking.HeureFermeture = dto.Ouvert24h24 ? null : dto.HeureFermeture;
        parking.ImageUrl = dto.ImageUrl;
        parking.Statut = ParseStatut(dto.Statut) ?? parking.Statut;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/parkings/5 (archivage/soft-delete + journalisation)
    // Regle : impossible si le parking a des acces recents.
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var parking = await _context.Parkings.FindAsync(id);
        if (parking is null)
            return NotFound();

        if (await AAccesRecents(id))
            return Conflict("Ce parking a des acces enregistres recemment : archivage impossible.");

        parking.EstSupprime = true;

        _context.HistoriqueParkings.Add(new HistoriqueParking
        {
            ParkingId = id,
            Action = "Archivage",
            Details = $"Parking '{parking.Nom}' (code {parking.Code}) archive."
        });

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // GET /api/parkings/5/historique - consultation de la journalisation
    [HttpGet("{id:int}/historique")]
    public async Task<ActionResult<IEnumerable<object>>> GetHistorique(int id)
    {
        var historique = await _context.HistoriqueParkings
            .Where(h => h.ParkingId == id)
            .OrderByDescending(h => h.DateAction)
            .Select(h => new { h.Id, h.Action, h.DateAction, h.Details })
            .ToListAsync();

        return Ok(historique);
    }

    // POST /api/parkings/5/zones
    [HttpPost("{parkingId:int}/zones")]
    public async Task<ActionResult<ZoneDto>> AjouterZone(int parkingId, CreateZoneDto dto)
    {
        var parking = await _context.Parkings.FindAsync(parkingId);
        if (parking is null)
            return NotFound($"Parking {parkingId} introuvable.");

        var zone = new Zone
        {
            ParkingId = parkingId,
            Nom = dto.Nom,
            Type = dto.Type,
            NombrePlaces = dto.NombrePlaces
        };

        _context.Zones.Add(zone);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = parkingId }, ToZoneDto(zone));
    }

    // PUT /api/parkings/zones/5
    [HttpPut("zones/{zoneId:int}")]
    public async Task<IActionResult> ModifierZone(int zoneId, CreateZoneDto dto)
    {
        var zone = await _context.Zones.FindAsync(zoneId);
        if (zone is null)
            return NotFound();

        if (dto.NombrePlaces < zone.PlacesOccupees)
            return BadRequest("Le nouveau nombre de places ne peut pas etre inferieur au nombre de places deja occupees.");

        zone.Nom = dto.Nom;
        zone.Type = dto.Type;
        zone.NombrePlaces = dto.NombrePlaces;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PUT /api/parkings/zones/5/statut - activer/desactiver une zone
    [HttpPut("zones/{zoneId:int}/statut")]
    public async Task<ActionResult<ZoneDto>> ChangerStatutZone(int zoneId, [FromBody] bool estActive)
    {
        var zone = await _context.Zones.FindAsync(zoneId);
        if (zone is null)
            return NotFound();

        zone.EstActive = estActive;
        await _context.SaveChangesAsync();

        return Ok(ToZoneDto(zone));
    }

    // DELETE /api/parkings/zones/5
    // Regle : un parking doit toujours garder au moins une zone.
    [HttpDelete("zones/{zoneId:int}")]
    public async Task<IActionResult> SupprimerZone(int zoneId)
    {
        var zone = await _context.Zones.FindAsync(zoneId);
        if (zone is null)
            return NotFound();

        var nombreZonesRestantes = await _context.Zones.CountAsync(z => z.ParkingId == zone.ParkingId);
        if (nombreZonesRestantes <= 1)
            return BadRequest("Impossible de supprimer la dernière zone : un parking doit avoir au moins une zone.");

        _context.Zones.Remove(zone);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PUT /api/parkings/zones/5/occupation
    // Temporaire : sera pilote par le module Acces une fois construit.
    [HttpPut("zones/{zoneId:int}/occupation")]
    public async Task<ActionResult<ZoneDto>> MettreAJourOccupation(int zoneId, UpdateOccupationZoneDto dto)
    {
        var zone = await _context.Zones.FindAsync(zoneId);
        if (zone is null)
            return NotFound();

        if (dto.PlacesOccupees > zone.NombrePlaces)
            return BadRequest("Le nombre de places occupees ne peut pas depasser le nombre de places de la zone.");

        zone.PlacesOccupees = dto.PlacesOccupees;
        await _context.SaveChangesAsync();

        return Ok(ToZoneDto(zone));
    }

    // ===== Aides internes =====

    // TODO : brancher sur la vraie table "Acces" une fois le module construit.
    // Pour l'instant, ne bloque jamais la suppression (retourne toujours false).
    private Task<bool> AAccesRecents(int parkingId)
    {
        return Task.FromResult(false);
    }

    private static StatutParking? ParseStatut(string? statut) =>
        Enum.TryParse<StatutParking>(statut, ignoreCase: true, out var resultat) ? resultat : null;

    private static ParkingDto ToDto(Parking p, bool inclureZones)
    {
        var capaciteMax = p.Zones.Sum(z => z.NombrePlaces);
        var placesOccupees = p.Zones.Sum(z => z.PlacesOccupees);

        return new ParkingDto
        {
            Id = p.Id,
            Nom = p.Nom,
            Code = p.Code,
            Adresse = p.Adresse,
            Latitude = p.Latitude,
            Longitude = p.Longitude,
            Ouvert24h24 = p.Ouvert24h24,
            HeureOuverture = p.HeureOuverture,
            HeureFermeture = p.HeureFermeture,
            ImageUrl = p.ImageUrl,
            Statut = p.Statut.ToString(),
            NombreZones = p.Zones.Count,
            CapaciteMax = capaciteMax,
            PlacesOccupees = placesOccupees,
            Zones = inclureZones ? p.Zones.Select(ToZoneDto).ToList() : new List<ZoneDto>()
        };
    }

    private static ZoneDto ToZoneDto(Zone z) => new()
    {
        Id = z.Id,
        Nom = z.Nom,
        Type = z.Type,
        EstActive = z.EstActive,
        NombrePlaces = z.NombrePlaces,
        PlacesOccupees = z.PlacesOccupees
    };
}
