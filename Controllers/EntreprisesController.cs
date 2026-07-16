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
public class EntreprisesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EntreprisesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET /api/entreprises?page=1&pageSize=10&search=xxx&statut=Actif
    [HasPermission(Permissions.EntreprisesRead)]
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

        var avecStatut = toutes
            .Select(e => new { Entreprise = e, Statut = CalculerStatutAbonnement(e) })
            .ToList();

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

    // Determine le statut affiche a partir de l'abonnement le plus recent de l'entreprise
    private static string CalculerStatutAbonnement(Entreprise entreprise)
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

        if (dernierAbonnement.Statut == StatutAbonnement.Suspendu)
            return "EnAttente";

        return "Actif";
    }

    // GET /api/entreprises/5
    [HasPermission(Permissions.EntreprisesRead)]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EntrepriseDto>> GetById(int id)
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

    // POST /api/entreprises
    [HasPermission(Permissions.EntreprisesWrite)]
    [HttpPost]
    public async Task<ActionResult<EntrepriseDto>> Create(CreateEntrepriseDto dto)
    {
        var existeDeja = await _context.Entreprises
            .AnyAsync(e => e.MatriculeFiscal == dto.MatriculeFiscal);

        if (existeDeja)
            return Conflict("Une entreprise avec ce matricule fiscal existe deja.");

        var entreprise = new Entreprise
        {
            Nom = dto.Nom,
            Adresse = dto.Adresse,
            Telephone = dto.Telephone,
            Email = dto.Email,
            MatriculeFiscal = dto.MatriculeFiscal,
            Secteur = dto.Secteur,
            DateCreation = DateTime.UtcNow
        };

        _context.Entreprises.Add(entreprise);
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

    // PUT /api/entreprises/5
    [HasPermission(Permissions.EntreprisesWrite)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateEntrepriseDto dto)
    {
        var entreprise = await _context.Entreprises.FindAsync(id);
        if (entreprise is null)
            return NotFound();

        entreprise.Nom = dto.Nom;
        entreprise.Adresse = dto.Adresse;
        entreprise.Telephone = dto.Telephone;
        entreprise.Email = dto.Email;
        entreprise.MatriculeFiscal = dto.MatriculeFiscal;
        entreprise.Secteur = dto.Secteur;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/entreprises/5 (soft delete)
    [HasPermission(Permissions.EntreprisesWrite)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entreprise = await _context.Entreprises.FindAsync(id);
        if (entreprise is null)
            return NotFound();

        entreprise.EstSupprime = true;
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
