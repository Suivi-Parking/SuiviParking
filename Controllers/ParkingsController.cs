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
    [HttpGet]
    public async Task<ActionResult<ParkingListeResponseDto>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var query = _context.Parkings
            .Include(p => p.Zones)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Nom.Contains(search) || p.Adresse.Contains(search));

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

    // GET /api/parkings/5 (avec le detail des zones)
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
    [HttpPost]
    public async Task<ActionResult<ParkingDto>> Create(CreateParkingDto dto)
    {
        var parking = new Parking
        {
            Nom = dto.Nom,
            Adresse = dto.Adresse,
            CapaciteMax = dto.CapaciteMax
        };

        _context.Parkings.Add(parking);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = parking.Id }, ToDto(parking, inclureZones: true));
    }

    // PUT /api/parkings/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateParkingDto dto)
    {
        var parking = await _context.Parkings.FindAsync(id);
        if (parking is null)
            return NotFound();

        parking.Nom = dto.Nom;
        parking.Adresse = dto.Adresse;
        parking.CapaciteMax = dto.CapaciteMax;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/parkings/5 (soft delete)
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var parking = await _context.Parkings.FindAsync(id);
        if (parking is null)
            return NotFound();

        parking.EstSupprime = true;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST /api/parkings/5/zones - ajouter une zone a un parking
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
            NombrePlaces = dto.NombrePlaces
        };

        _context.Zones.Add(zone);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = parkingId }, ToZoneDto(zone));
    }

    // PUT /api/parkings/zones/5 - modifier une zone (nom / nombre de places)
    [HttpPut("zones/{zoneId:int}")]
    public async Task<IActionResult> ModifierZone(int zoneId, CreateZoneDto dto)
    {
        var zone = await _context.Zones.FindAsync(zoneId);
        if (zone is null)
            return NotFound();

        if (dto.NombrePlaces < zone.PlacesOccupees)
            return BadRequest("Le nouveau nombre de places ne peut pas etre inferieur au nombre de places deja occupees.");

        zone.Nom = dto.Nom;
        zone.NombrePlaces = dto.NombrePlaces;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/parkings/zones/5
    [HttpDelete("zones/{zoneId:int}")]
    public async Task<IActionResult> SupprimerZone(int zoneId)
    {
        var zone = await _context.Zones.FindAsync(zoneId);
        if (zone is null)
            return NotFound();

        _context.Zones.Remove(zone);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PUT /api/parkings/zones/5/occupation - ajuster manuellement l'occupation
    // (temporaire : sera pilote par le module Acces plus tard)
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

    private static ParkingDto ToDto(Parking p, bool inclureZones)
    {
        var placesTotales = p.Zones.Sum(z => z.NombrePlaces);
        var placesOccupees = p.Zones.Sum(z => z.PlacesOccupees);

        return new ParkingDto
        {
            Id = p.Id,
            Nom = p.Nom,
            Adresse = p.Adresse,
            CapaciteMax = p.CapaciteMax,
            NombreZones = p.Zones.Count,
            PlacesTotales = placesTotales,
            PlacesOccupees = placesOccupees,
            Zones = inclureZones ? p.Zones.Select(ToZoneDto).ToList() : new List<ZoneDto>()
        };
    }

    private static ZoneDto ToZoneDto(Zone z) => new()
    {
        Id = z.Id,
        Nom = z.Nom,
        NombrePlaces = z.NombrePlaces,
        PlacesOccupees = z.PlacesOccupees
    };
}