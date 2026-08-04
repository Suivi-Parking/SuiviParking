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
public class AbonnementsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AbonnementsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/abonnements?entrepriseId=1&statut=Actif&page=1&pageSize=10
    [HasPermission(Permissions.AbonnementsRead)]
    [HttpGet]
    public async Task<ActionResult<AbonnementListeResponseDto>> GetAll(
        [FromQuery] int? entrepriseId = null,
        [FromQuery] string? statut = null, // "Actif" | "Suspendu" | "Resilie" | "Expire"
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Abonnements
            .Include(a => a.Entreprise)
            .AsQueryable();

        if (entrepriseId.HasValue)
            query = query.Where(a => a.EntrepriseId == entrepriseId.Value);

        var toutes = await query.ToListAsync();

        var avecStatut = toutes
            .Select(a => new { Abonnement = a, StatutEffectif = CalculerStatutEffectif(a) })
            .ToList();

        var counts = new AbonnementCountsDto
        {
            Tous = avecStatut.Count,
            Actif = avecStatut.Count(x => x.StatutEffectif == "Actif"),
            Suspendu = avecStatut.Count(x => x.StatutEffectif == "Suspendu"),
            Resilie = avecStatut.Count(x => x.StatutEffectif == "Resilie"),
            Expire = avecStatut.Count(x => x.StatutEffectif == "Expire")
        };

        if (!string.IsNullOrWhiteSpace(statut))
            avecStatut = avecStatut.Where(x => x.StatutEffectif == statut).ToList();

        var totalCount = avecStatut.Count;

        var items = avecStatut
            .OrderByDescending(x => x.Abonnement.DateDebut)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToDto(x.Abonnement, x.StatutEffectif))
            .ToList();

        return Ok(new AbonnementListeResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Counts = counts
        });
    }

    // GET /api/abonnements/5
    [HasPermission(Permissions.AbonnementsRead)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AbonnementDto>> GetById(int id)
    {
        var abonnement = await _context.Abonnements
            .Include(a => a.Entreprise)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (abonnement is null)
            return NotFound();

        return Ok(ToDto(abonnement, CalculerStatutEffectif(abonnement)));
    }

    // POST /api/abonnements
    // Regle metier : une entreprise ne peut pas avoir 2 abonnements actifs en meme temps.
    [HasPermission(Permissions.AbonnementsWrite)]
    [HttpPost]
    public async Task<ActionResult<AbonnementDto>> Create(CreateAbonnementDto dto)
    {
        var entreprise = await _context.Entreprises.FindAsync(dto.EntrepriseId);
        if (entreprise is null)
            return NotFound($"Entreprise {dto.EntrepriseId} introuvable.");

        var abonnementsExistants = await _context.Abonnements
            .Where(a => a.EntrepriseId == dto.EntrepriseId)
            .ToListAsync();

        var aDejaUnAbonnementActif = abonnementsExistants
            .Any(a => CalculerStatutEffectif(a) == "Actif");

        if (aDejaUnAbonnementActif)
            return Conflict("Cette entreprise a deja un abonnement actif. Suspendez ou resiliez-le avant d'en creer un nouveau.");

        if (dto.DateFin <= dto.DateDebut)
            return BadRequest("La date de fin doit etre posterieure a la date de debut.");

        var abonnement = new Abonnement
        {
            EntrepriseId = dto.EntrepriseId,
            ParkingId = dto.ParkingId,
            DateDebut = DateTime.SpecifyKind(dto.DateDebut, DateTimeKind.Utc),
            DateFin = DateTime.SpecifyKind(dto.DateFin, DateTimeKind.Utc),
            TypeAbonnement = dto.TypeAbonnement,
            MontantMensuel = dto.MontantMensuel,
            Statut = StatutAbonnement.Actif
        };

        _context.Abonnements.Add(abonnement);
        await _context.SaveChangesAsync();

        abonnement.Entreprise = entreprise;
        return CreatedAtAction(nameof(GetById), new { id = abonnement.Id },
            ToDto(abonnement, CalculerStatutEffectif(abonnement)));
    }

    // POST /api/abonnements/5/renouveler
    // Autorise uniquement si le statut effectif actuel est "Expire".
    [HasPermission(Permissions.AbonnementsWrite)]
    [HttpPost("{id:int}/renouveler")]
    public async Task<ActionResult<AbonnementDto>> Renouveler(int id, RenouvelerAbonnementDto dto)
    {
        var abonnement = await _context.Abonnements
            .Include(a => a.Entreprise)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (abonnement is null)
            return NotFound();

        if (CalculerStatutEffectif(abonnement) != "Expire")
            return BadRequest("Seul un abonnement expire peut etre renouvele.");

        if (dto.NouvelleDateFin <= DateTime.UtcNow)
            return BadRequest("La nouvelle date de fin doit etre dans le futur.");

        abonnement.DateDebut = DateTime.UtcNow;
        abonnement.DateFin = dto.NouvelleDateFin.ToUniversalTime();
        abonnement.Statut = StatutAbonnement.Actif;
        abonnement.Raison = null;

        await _context.SaveChangesAsync();

        return Ok(ToDto(abonnement, CalculerStatutEffectif(abonnement)));
    }

    // POST /api/abonnements/5/suspendre
    [HasPermission(Permissions.AbonnementsWrite)]
    [HttpPost("{id:int}/suspendre")]
    public async Task<ActionResult<AbonnementDto>> Suspendre(int id, ChangerStatutAbonnementDto dto)
    {
        var abonnement = await _context.Abonnements
            .Include(a => a.Entreprise)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (abonnement is null)
            return NotFound();

        var statutActuel = CalculerStatutEffectif(abonnement);
        if (statutActuel != "Actif")
            return BadRequest("Seul un abonnement actif peut etre suspendu.");

        abonnement.Statut = StatutAbonnement.Suspendu;
        abonnement.Raison = dto.Raison;

        await _context.SaveChangesAsync();

        return Ok(ToDto(abonnement, CalculerStatutEffectif(abonnement)));
    }

    // POST /api/abonnements/5/resilier
    [HasPermission(Permissions.AbonnementsWrite)]
    [HttpPost("{id:int}/resilier")]
    public async Task<ActionResult<AbonnementDto>> Resilier(int id, ChangerStatutAbonnementDto dto)
    {
        var abonnement = await _context.Abonnements
            .Include(a => a.Entreprise)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (abonnement is null)
            return NotFound();

        var statutActuel = CalculerStatutEffectif(abonnement);
        if (statutActuel != "Actif" && statutActuel != "Suspendu")
            return BadRequest("Seul un abonnement actif ou suspendu peut etre resilie.");

        abonnement.Statut = StatutAbonnement.Resilie;
        abonnement.Raison = dto.Raison;

        await _context.SaveChangesAsync();

        return Ok(ToDto(abonnement, CalculerStatutEffectif(abonnement)));
    }

    // Le statut "effectif" tient compte de la date de fin sans avoir besoin
    // d'une tache planifiee qui repasserait les abonnements en "Expire" en base.
    private static string CalculerStatutEffectif(Abonnement abonnement)
    {
        if (abonnement.Statut == StatutAbonnement.Actif && abonnement.DateFin < DateTime.UtcNow)
            return "Expire";

        return abonnement.Statut.ToString();
    }

    private static AbonnementDto ToDto(Abonnement a, string statutEffectif) => new()
    {
        Id = a.Id,
        EntrepriseId = a.EntrepriseId,
        EntrepriseNom = a.Entreprise?.Nom ?? string.Empty,
        ParkingId = a.ParkingId,
        DateDebut = a.DateDebut,
        DateFin = a.DateFin,
        TypeAbonnement = a.TypeAbonnement,
        MontantMensuel = a.MontantMensuel,
        Raison = a.Raison,
        Statut = statutEffectif
    };
}
