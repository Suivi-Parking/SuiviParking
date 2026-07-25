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
public class PaiementsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    // Nombre de jours de retard avant suspension automatique de l'abonnement.
    private const int JoursAvantSuspensionParDefaut = 15;

    public PaiementsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/paiements?entrepriseId=1&abonnementId=2&statut=EnRetard&page=1&pageSize=10
    [HasPermission(Permissions.PaiementsRead)]
    [HttpGet]
    public async Task<ActionResult<PaiementListeResponseDto>> GetAll(
        [FromQuery] int? entrepriseId = null,
        [FromQuery] int? abonnementId = null,
        [FromQuery] string? statut = null, // "Paye" | "EnAttente" | "EnRetard" | "Rejete"
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = _context.Paiements
            .Include(p => p.Abonnement).ThenInclude(a => a!.Entreprise)
            .AsQueryable();

        if (entrepriseId.HasValue)
            query = query.Where(p => p.Abonnement!.EntrepriseId == entrepriseId.Value);

        if (abonnementId.HasValue)
            query = query.Where(p => p.AbonnementId == abonnementId.Value);

        var tous = await query.ToListAsync();

        var avecStatut = tous
            .Select(p => new { Paiement = p, StatutEffectif = CalculerStatutEffectif(p) })
            .ToList();

        var counts = new PaiementCountsDto
        {
            Tous = avecStatut.Count,
            Paye = avecStatut.Count(x => x.StatutEffectif == "Paye"),
            EnAttente = avecStatut.Count(x => x.StatutEffectif == "EnAttente"),
            EnRetard = avecStatut.Count(x => x.StatutEffectif == "EnRetard"),
            Rejete = avecStatut.Count(x => x.StatutEffectif == "Rejete")
        };

        if (!string.IsNullOrWhiteSpace(statut))
            avecStatut = avecStatut.Where(x => x.StatutEffectif == statut).ToList();

        var totalCount = avecStatut.Count;

        var items = avecStatut
            .OrderBy(x => x.Paiement.DateEcheance)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => ToDto(x.Paiement, x.StatutEffectif))
            .ToList();

        return Ok(new PaiementListeResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Counts = counts
        });
    }

    // GET /api/paiements/5
    [HasPermission(Permissions.PaiementsRead)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PaiementDto>> GetById(int id)
    {
        var paiement = await _context.Paiements
            .Include(p => p.Abonnement).ThenInclude(a => a!.Entreprise)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (paiement is null)
            return NotFound();

        return Ok(ToDto(paiement, CalculerStatutEffectif(paiement)));
    }

    // GET /api/paiements/echeances
    // Prochaines echeances non payees, triees par urgence.
    [HasPermission(Permissions.PaiementsRead)]
    [HttpGet("echeances")]
    public async Task<ActionResult<List<PaiementDto>>> GetEcheances()
    {
        var paiements = await _context.Paiements
            .Include(p => p.Abonnement).ThenInclude(a => a!.Entreprise)
            .Where(p => p.Statut == StatutPaiement.EnAttente)
            .ToListAsync();

        var items = paiements
            .Select(p => ToDto(p, CalculerStatutEffectif(p)))
            .OrderBy(p => p.DateEcheance)
            .ToList();

        return Ok(items);
    }

    // GET /api/paiements/indicateurs?entrepriseId=1
    [HasPermission(Permissions.PaiementsRead)]
    [HttpGet("indicateurs")]
    public async Task<ActionResult<IndicateursFinanciersDto>> GetIndicateurs([FromQuery] int? entrepriseId = null)
    {
        var query = _context.Paiements.Include(p => p.Abonnement).AsQueryable();

        if (entrepriseId.HasValue)
            query = query.Where(p => p.Abonnement!.EntrepriseId == entrepriseId.Value);

        var tous = await query.ToListAsync();

        var montantPercu = tous.Where(p => p.Statut == StatutPaiement.Paye).Sum(p => p.Montant);
        var montantImpaye = tous
            .Where(p => CalculerStatutEffectif(p) is "EnAttente" or "EnRetard")
            .Sum(p => p.Montant);

        var totalDu = montantPercu + montantImpaye;
        var tauxRecouvrement = totalDu > 0 ? (double)(montantPercu / totalDu) * 100 : 0;

        var repartition = tous
            .Where(p => p.Statut == StatutPaiement.Paye && p.ModePaiement.HasValue)
            .GroupBy(p => p.ModePaiement!.Value.ToString())
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Montant));

        return Ok(new IndicateursFinanciersDto
        {
            MontantTotalPercu = montantPercu,
            MontantImpaye = montantImpaye,
            TauxRecouvrement = Math.Round(tauxRecouvrement, 1),
            RepartitionParModePaiement = repartition
        });
    }

    // POST /api/paiements
    // Cree une echeance de paiement (statut EnAttente par defaut).
    [HasPermission(Permissions.PaiementsWrite)]
    [HttpPost]
    public async Task<ActionResult<PaiementDto>> Create(CreatePaiementDto dto)
    {
        var abonnement = await _context.Abonnements
            .Include(a => a.Entreprise)
            .FirstOrDefaultAsync(a => a.Id == dto.AbonnementId);

        if (abonnement is null)
            return NotFound($"Abonnement {dto.AbonnementId} introuvable.");

        if (dto.Montant <= 0)
            return BadRequest("Le montant doit etre superieur a 0.");

        var paiement = new Paiement
        {
            AbonnementId = dto.AbonnementId,
            Montant = dto.Montant,
            DateEcheance = DateTime.SpecifyKind(dto.DateEcheance, DateTimeKind.Utc),
            PeriodeCouverte = dto.PeriodeCouverte,
            Statut = StatutPaiement.EnAttente
        };

        _context.Paiements.Add(paiement);
        await _context.SaveChangesAsync();

        paiement.Abonnement = abonnement;
        return CreatedAtAction(nameof(GetById), new { id = paiement.Id },
            ToDto(paiement, CalculerStatutEffectif(paiement)));
    }

    // POST /api/paiements/5/marquer-paye
    [HasPermission(Permissions.PaiementsWrite)]
    [HttpPost("{id:int}/marquer-paye")]
    public async Task<ActionResult<PaiementDto>> MarquerPaye(int id, MarquerPayeDto dto)
    {
        var paiement = await _context.Paiements
            .Include(p => p.Abonnement).ThenInclude(a => a!.Entreprise)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (paiement is null)
            return NotFound();

        if (paiement.Statut == StatutPaiement.Paye)
            return BadRequest("Ce paiement est deja marque comme paye.");

        if (!Enum.TryParse<ModePaiement>(dto.ModePaiement, out var mode))
            return BadRequest("Mode de paiement invalide.");

        paiement.Statut = StatutPaiement.Paye;
        paiement.DatePaiement = DateTime.SpecifyKind(dto.DatePaiement, DateTimeKind.Utc);
        paiement.ModePaiement = mode;
        paiement.ReferenceTransaction = dto.ReferenceTransaction;

        await _context.SaveChangesAsync();

        return Ok(ToDto(paiement, CalculerStatutEffectif(paiement)));
    }

    // POST /api/paiements/5/rejeter
    [HasPermission(Permissions.PaiementsWrite)]
    [HttpPost("{id:int}/rejeter")]
    public async Task<ActionResult<PaiementDto>> Rejeter(int id, RejeterPaiementDto dto)
    {
        var paiement = await _context.Paiements
            .Include(p => p.Abonnement).ThenInclude(a => a!.Entreprise)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (paiement is null)
            return NotFound();

        paiement.Statut = StatutPaiement.Rejete;

        await _context.SaveChangesAsync();

        return Ok(ToDto(paiement, CalculerStatutEffectif(paiement)));
    }

    // POST /api/paiements/verifier-retards?joursAvantSuspension=15
    // Detecte les echeances en retard et suspend automatiquement les abonnements
    // dont un paiement est en retard depuis plus de N jours.
    [HasPermission(Permissions.PaiementsWrite)]
    [HttpPost("verifier-retards")]
    public async Task<ActionResult<object>> VerifierRetards([FromQuery] int joursAvantSuspension = JoursAvantSuspensionParDefaut)
    {
        var enAttente = await _context.Paiements
            .Include(p => p.Abonnement)
            .Where(p => p.Statut == StatutPaiement.EnAttente)
            .ToListAsync();

        var enRetard = enAttente
            .Where(p => (DateTime.UtcNow - p.DateEcheance).TotalDays > 0)
            .ToList();

        var abonnementsSuspendus = new List<int>();

        foreach (var paiement in enRetard)
        {
            var joursDeRetard = (int)(DateTime.UtcNow - paiement.DateEcheance).TotalDays;

            if (joursDeRetard >= joursAvantSuspension
                && paiement.Abonnement is not null
                && paiement.Abonnement.Statut == StatutAbonnement.Actif)
            {
                paiement.Abonnement.Statut = StatutAbonnement.Suspendu;
                paiement.Abonnement.Raison = $"Suspendu automatiquement : paiement en retard de {joursDeRetard} jours.";
                abonnementsSuspendus.Add(paiement.Abonnement.Id);
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            paiementsEnRetard = enRetard.Count,
            abonnementsSuspendus = abonnementsSuspendus.Count,
            details = abonnementsSuspendus
        });
    }

    // Le statut "effectif" detecte le retard sans avoir besoin d'une tache planifiee,
    // sur le meme principe que CalculerStatutEffectif dans AbonnementsController.
    private static string CalculerStatutEffectif(Paiement p)
    {
        if (p.Statut == StatutPaiement.EnAttente && p.DateEcheance < DateTime.UtcNow)
            return "EnRetard";

        return p.Statut.ToString();
    }

    private static int CalculerJoursRetard(Paiement p)
    {
        if (CalculerStatutEffectif(p) != "EnRetard")
            return 0;

        return (int)(DateTime.UtcNow - p.DateEcheance).TotalDays;
    }

    private static PaiementDto ToDto(Paiement p, string statutEffectif) => new()
    {
        Id = p.Id,
        AbonnementId = p.AbonnementId,
        EntrepriseNom = p.Abonnement?.Entreprise?.Nom ?? string.Empty,
        TypeAbonnement = p.Abonnement?.TypeAbonnement ?? string.Empty,
        Montant = p.Montant,
        DateEcheance = p.DateEcheance,
        DatePaiement = p.DatePaiement,
        ModePaiement = p.ModePaiement?.ToString(),
        ReferenceTransaction = p.ReferenceTransaction,
        PeriodeCouverte = p.PeriodeCouverte,
        Statut = statutEffectif,
        JoursRetard = CalculerJoursRetard(p)
    };
}