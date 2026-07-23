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
public class EmployeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET api/employe
    [HasPermission(Permissions.EmployesRead)]
    [HttpGet]
    public async Task<ActionResult<EmployeListeResponseDto>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? entrepriseId = null)
    {
        var query = _context.Employes
            .Include(e => e.Vehicules)
            .AsQueryable();

        if (entrepriseId.HasValue)
            query = query.Where(e => e.EntrepriseId == entrepriseId.Value);

        var total = await query.CountAsync();

        var employes = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(e => new EmployeDto
            {
                Id = e.Id,
                Nom = e.Nom,
                Prenom = e.Prenom,
                Email = e.Email,
                Telephone = e.Telephone,
                Fonction = e.Fonction,
                Statut = e.Statut,
                DateRattachement = e.DateRattachement,
                EntrepriseId = e.EntrepriseId,
                Vehicules = e.Vehicules.Select(v => new VehiculeDto
                {
                    Id = v.Id,
                    Immatriculation = v.Immatriculation,
                    Marque = v.Marque,
                    Modele = v.Modele,
                    Couleur = v.Couleur,
                    Type = v.Type,
                    Statut = v.Statut,
                    EmployeId = v.EmployeId
                }).ToList()
            })
            .ToListAsync();

        return Ok(new EmployeListeResponseDto
        {
            Items = employes,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        });
    }

    // GET api/employe/5
    [HasPermission(Permissions.EmployesRead)]
    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeDto>> GetById(int id)
    {
        var e = await _context.Employes
            .Include(e => e.Vehicules)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (e == null)
            return NotFound(new { message = $"Employé avec l'ID {id} introuvable !" });

        return Ok(new EmployeDto
        {
            Id = e.Id,
            Nom = e.Nom,
            Prenom = e.Prenom,
            Email = e.Email,
            Telephone = e.Telephone,
            Fonction = e.Fonction,
            Statut = e.Statut,
            DateRattachement = e.DateRattachement,
            EntrepriseId = e.EntrepriseId,
            Vehicules = e.Vehicules.Select(v => new VehiculeDto
            {
                Id = v.Id,
                Immatriculation = v.Immatriculation,
                Marque = v.Marque,
                Modele = v.Modele,
                Couleur = v.Couleur,
                Type = v.Type,
                Statut = v.Statut,
                EmployeId = v.EmployeId
            }).ToList()
        });
    }

    // POST api/employe
    [HasPermission(Permissions.EmployesWrite)]
    [HttpPost]
    public async Task<ActionResult<EmployeDto>> Create(CreateEmployeDto dto)
    {
        if (await _context.Employes.AnyAsync(e => e.Email == dto.Email))
            return Conflict(new { message = "Cet email est déjà utilisé par un autre employé !" });

        var employe = new Employe
        {
            Nom = dto.Nom,
            Prenom = dto.Prenom,
            Email = dto.Email,
            Telephone = dto.Telephone,
            Fonction = dto.Fonction,
            Statut = "ACTIF",
            DateRattachement = DateTime.UtcNow,
            EntrepriseId = dto.EntrepriseId
        };

        _context.Employes.Add(employe);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById),
            new { id = employe.Id },
            new EmployeDto
            {
                Id = employe.Id,
                Nom = employe.Nom,
                Prenom = employe.Prenom,
                Email = employe.Email,
                Telephone = employe.Telephone,
                Fonction = employe.Fonction,
                Statut = employe.Statut,
                DateRattachement = employe.DateRattachement,
                EntrepriseId = employe.EntrepriseId
            });
    }

    // PUT api/employe/5
    [HasPermission(Permissions.EmployesWrite)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateEmployeDto dto)
    {
        if (id != dto.Id)
            return BadRequest(new { message = "L'ID de l'URL ne correspond pas à l'ID du corps !" });

        var employe = await _context.Employes.FindAsync(id);
        if (employe == null)
            return NotFound(new { message = $"Employé avec l'ID {id} introuvable !" });

        if (await _context.Employes
            .AnyAsync(e => e.Email == dto.Email && e.Id != id))
            return Conflict(new { message = "Cet email est déjà utilisé par un autre employé !" });

        employe.Nom = dto.Nom;
        employe.Prenom = dto.Prenom;
        employe.Email = dto.Email;
        employe.Telephone = dto.Telephone;
        employe.Fonction = dto.Fonction;
        employe.Statut = dto.Statut;
        employe.EntrepriseId = dto.EntrepriseId;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE api/employe/5
    [HasPermission(Permissions.EmployesWrite)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var employe = await _context.Employes
            .Include(e => e.Vehicules)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employe == null)
            return NotFound(new { message = $"Employé avec l'ID {id} introuvable !" });

        if (employe.Vehicules.Any())
            return BadRequest(new { message = $"Impossible de supprimer cet employé car il possède {employe.Vehicules.Count} véhicule(s) !" });

        _context.Employes.Remove(employe);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
