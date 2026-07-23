using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parkini.Application.DTOs;
using Parkini.Entities;
using Parkini.Infrastructure.Data;
using Parkini.Infrastructure.Security;

namespace Parkini.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ParkingsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EntreprisesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/parkings?page=1&pageSize=10&search=xxx
    [HasPermission(Permissions.ParkingsRead)]
    [HttpGet]
    public async Task<ActionResult<EntrepriseListeResponseDto>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? statut = null) // "Actif" | "EnAttente" | "Expire"
    {
        var query = _context.Entreprises
            .Include(e => e.Abonnements)
            // Employes volontairement non charge : la table n'existe pas encore.
            // NombreEmployes restera a 0 jusqu'au ticket "gestion des employes".
            .AsQueryable(); // le filtre soft-delete est global (voir DbContext)

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e =>
                e.Nom.Contains(search) ||
                e.MatriculeFiscal.Contains(search));
        }

        // Le statut est calcule en memoire (pas directement traduisible en SQL simple).
        // Suffisant pour le volume attendu sur ce projet ; a revoir via une colonne
        // calculee ou une vue SQL si le nombre d'entreprises devient tres important.
        var toutes = await query.ToListAsync();

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

    // GET /api/parkings/5
    [HasPermission(Permissions.ParkingsRead)]
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
    [HasPermission(Permissions.ParkingsWrite)]
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

        var counts = new EntrepriseCountsDto
        {
            Tous = avecStatut.Count,
            Actif = avecStatut.Count(x => x.Statut == "Actif"),
            EnAttente = avecStatut.Count(x => x.Statut == "EnAttente"),
            Expire = avecStatut.Count(x => x.Statut == "Expire")
        };

        if (!string.IsNullOrWhiteSpace(statut))
        {
            avecStatut = avecStatut.Where(x => x.Statut == statut).ToList();
        }

        var totalCount = avecStatut.Count;

        var items = avecStatut
            .OrderBy(x => x.Entreprise.Nom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new EntrepriseDto
            {
                Id = x.Entreprise.Id,
                Nom = x.Entreprise.Nom,
                Adresse = x.Entreprise.Adresse,
                Telephone = x.Entreprise.Telephone,
                Email = x.Entreprise.Email,
                MatriculeFiscal = x.Entreprise.MatriculeFiscal,
                Secteur = x.Entreprise.Secteur,
                DateCreation = x.Entreprise.DateCreation,
                NombreEmployes = 0, // place-holder tant que le module Employes n'existe pas
                StatutAbonnement = x.Statut
            })
            .ToList();

        return Ok(new EntrepriseListeResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Counts = counts
        });
    }

    // PUT /api/parkings/5
    [HasPermission(Permissions.ParkingsWrite)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateParkingDto dto)
    {
        var dernierAbonnement = entreprise.Abonnements
            .OrderByDescending(a => a.DateFin)
            .FirstOrDefault();

        if (dernierAbonnement is null)
            return "EnAttente"; // aucune souscription encore creee

        if (dernierAbonnement.Statut == StatutAbonnement.Resilie ||
            dernierAbonnement.Statut == StatutAbonnement.Expire ||
            dernierAbonnement.DateFin < DateTime.UtcNow)
            return "Expire";

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
        parking.DateDerniereModification = DateTime.UtcNow;

        return "Actif";
    }

    // DELETE /api/parkings/5 (archivage + journalisation)
    [HasPermission(Permissions.ParkingsWrite)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entreprise = await _context.Entreprises
            .Include(e => e.Abonnements)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entreprise is null)
            return NotFound();

        return Ok(new EntrepriseDto
        {
            Id = entreprise.Id,
            Nom = entreprise.Nom,
            Adresse = entreprise.Adresse,
            Telephone = entreprise.Telephone,
            Email = entreprise.Email,
            MatriculeFiscal = entreprise.MatriculeFiscal,
            Secteur = entreprise.Secteur,
            DateCreation = entreprise.DateCreation,
            NombreEmployes = 0, // place-holder tant que le module Employes n'existe pas
            StatutAbonnement = CalculerStatutAbonnement(entreprise)
        });
    }

    // GET /api/parkings/5/historique
    [HasPermission(Permissions.ParkingsRead)]
    [HttpGet("{id:int}/historique")]
    public async Task<ActionResult<IEnumerable<object>>> GetHistorique(int id)
    {
        var existeDeja = await _context.Entreprises
            .AnyAsync(e => e.MatriculeFiscal == dto.MatriculeFiscal);

        if (existeDeja)
            return Conflict("Une entreprise avec ce matricule fiscal existe deja.");

    // POST /api/parkings/5/zones
    [HasPermission(Permissions.ParkingsWrite)]
    [HttpPost("{parkingId:int}/zones")]
    public async Task<ActionResult<ZoneDto>> AjouterZone(int parkingId, CreateZoneDto dto)
    {
        var parking = await _context.Parkings.FindAsync(parkingId);
        if (parking is null)
            return NotFound($"Parking {parkingId} introuvable.");

        var zone = new Zone
        {
            Nom = dto.Nom,
            Adresse = dto.Adresse,
            Telephone = dto.Telephone,
            Email = dto.Email,
            MatriculeFiscal = dto.MatriculeFiscal,
            Secteur = dto.Secteur,
            DateCreation = DateTime.UtcNow
        };

        _context.Zones.Add(zone);
        parking.DateDerniereModification = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entreprise.Id }, new EntrepriseDto
        {
            Id = entreprise.Id,
            Nom = entreprise.Nom,
            Adresse = entreprise.Adresse,
            Telephone = entreprise.Telephone,
            Email = entreprise.Email,
            MatriculeFiscal = entreprise.MatriculeFiscal,
            Secteur = entreprise.Secteur,
            DateCreation = entreprise.DateCreation,
            NombreEmployes = 0,
            StatutAbonnement = "EnAttente"
        });
    }

    // PUT /api/parkings/zones/5
    [HasPermission(Permissions.ParkingsWrite)]
    [HttpPut("zones/{zoneId:int}")]
    public async Task<IActionResult> ModifierZone(int zoneId, CreateZoneDto dto)
    {
        var zone = await _context.Zones.Include(z => z.Parking).FirstOrDefaultAsync(z => z.Id == zoneId);
        if (zone is null)
            return NotFound();

        entreprise.Nom = dto.Nom;
        entreprise.Adresse = dto.Adresse;
        entreprise.Telephone = dto.Telephone;
        entreprise.Email = dto.Email;
        entreprise.MatriculeFiscal = dto.MatriculeFiscal;
        entreprise.Secteur = dto.Secteur;

        if (zone.Parking is not null)
            zone.Parking.DateDerniereModification = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PUT /api/parkings/zones/5/statut
    [HasPermission(Permissions.ParkingsWrite)]
    [HttpPut("zones/{zoneId:int}/statut")]
    public async Task<ActionResult<ZoneDto>> ChangerStatutZone(int zoneId, [FromBody] bool estActive)
    {
        var zone = await _context.Zones.Include(z => z.Parking).FirstOrDefaultAsync(z => z.Id == zoneId);
        if (zone is null)
            return NotFound();

        zone.EstActive = estActive;

        if (zone.Parking is not null)
            zone.Parking.DateDerniereModification = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(ToZoneDto(zone));
    }

    // DELETE /api/parkings/zones/5
    [HasPermission(Permissions.ParkingsWrite)]
    [HttpDelete("zones/{zoneId:int}")]
    public async Task<IActionResult> SupprimerZone(int zoneId)
    {
        var zone = await _context.Zones.Include(z => z.Parking).FirstOrDefaultAsync(z => z.Id == zoneId);
        if (zone is null)
            return NotFound();

        var nombreZonesRestantes = await _context.Zones.CountAsync(z => z.ParkingId == zone.ParkingId);
        if (nombreZonesRestantes <= 1)
            return BadRequest("Impossible de supprimer la dernière zone : un parking doit avoir au moins une zone.");

        _context.Zones.Remove(zone);

        if (zone.Parking is not null)
            zone.Parking.DateDerniereModification = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // PUT /api/parkings/zones/5/occupation
    [HasPermission(Permissions.ParkingsWrite)]
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
            DateDerniereModification = p.DateDerniereModification,
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
