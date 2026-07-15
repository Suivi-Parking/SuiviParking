using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parkini.Application.DTOs;
using Parkini.Entities;
using Parkini.Infrastructure.Data;

namespace Parkini.Controllers;


    [ApiController]
    [Route("api/[controller]")]
    public class VehiculeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VehiculeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/vehicule
        [HttpGet]
        public async Task<ActionResult<VehiculeListeResponseDto>> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? employeId = null)
        {
            var query = _context.Vehicules
                .Include(v => v.Employe)
                .AsQueryable();

            if (employeId.HasValue)
                query = query.Where(v => v.EmployeId == employeId.Value);

            var total = await query.CountAsync();

            var vehicules = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(v => new VehiculeDto
                {
                    Id = v.Id,
                    Immatriculation = v.Immatriculation,
                    Marque = v.Marque,
                    Modele = v.Modele,
                    Couleur = v.Couleur,
                    Type = v.Type,
                    Statut = v.Statut,
                    EmployeId = v.EmployeId,
                    EmployeNom = v.Employe != null ?
                        v.Employe.Nom + " " + v.Employe.Prenom : ""
                })
                .ToListAsync();

            return Ok(new VehiculeListeResponseDto
            {
                Items = vehicules,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

        // GET api/vehicule/5
        [HttpGet("{id}")]
        public async Task<ActionResult<VehiculeDto>> GetById(int id)
        {
            var v = await _context.Vehicules
                .Include(v => v.Employe)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (v == null)
                return NotFound(new { message = $"Véhicule avec l'ID {id} introuvable !" });

            return Ok(new VehiculeDto
            {
                Id = v.Id,
                Immatriculation = v.Immatriculation,
                Marque = v.Marque,
                Modele = v.Modele,
                Couleur = v.Couleur,
                Type = v.Type,
                Statut = v.Statut,
                EmployeId = v.EmployeId,
                EmployeNom = v.Employe != null ?
                    v.Employe.Nom + " " + v.Employe.Prenom : ""
            });
        }

        // POST api/vehicule
        [HttpPost]
        public async Task<ActionResult<VehiculeDto>> Create(CreateVehiculeDto dto)
        {
            if (await _context.Vehicules
                .AnyAsync(v => v.Immatriculation == dto.Immatriculation))
                return Conflict(new { message = "Cette immatriculation est déjà utilisée !" });

            if (!await _context.Employes.AnyAsync(e => e.Id == dto.EmployeId))
                return NotFound(new { message = $"Employé avec l'ID {dto.EmployeId} introuvable !" });

            var vehicule = new Vehicule
            {
                Immatriculation = dto.Immatriculation,
                Marque = dto.Marque,
                Modele = dto.Modele,
                Couleur = dto.Couleur,
                Type = dto.Type,
                Statut = "ACTIF",
                EmployeId = dto.EmployeId
            };

            _context.Vehicules.Add(vehicule);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = vehicule.Id },
                new VehiculeDto
                {
                    Id = vehicule.Id,
                    Immatriculation = vehicule.Immatriculation,
                    Marque = vehicule.Marque,
                    Modele = vehicule.Modele,
                    Couleur = vehicule.Couleur,
                    Type = vehicule.Type,
                    Statut = vehicule.Statut,
                    EmployeId = vehicule.EmployeId
                });
        }

        // PUT api/vehicule/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateVehiculeDto dto)
        {
            if (id != dto.Id)
                return BadRequest(new { message = "L'ID de l'URL ne correspond pas à l'ID du corps !" });

            var vehicule = await _context.Vehicules.FindAsync(id);
            if (vehicule == null)
                return NotFound(new { message = $"Véhicule avec l'ID {id} introuvable !" });

            if (await _context.Vehicules
                .AnyAsync(v => v.Immatriculation == dto.Immatriculation && v.Id != id))
                return Conflict(new { message = "Cette immatriculation est déjà utilisée par un autre véhicule !" });

            if (!await _context.Employes.AnyAsync(e => e.Id == dto.EmployeId))
                return NotFound(new { message = $"Employé avec l'ID {dto.EmployeId} introuvable !" });

            vehicule.Immatriculation = dto.Immatriculation;
            vehicule.Marque = dto.Marque;
            vehicule.Modele = dto.Modele;
            vehicule.Couleur = dto.Couleur;
            vehicule.Type = dto.Type;
            vehicule.Statut = dto.Statut;
            vehicule.EmployeId = dto.EmployeId;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/vehicule/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicule = await _context.Vehicules.FindAsync(id);
            if (vehicule == null)
                return NotFound(new { message = $"Véhicule avec l'ID {id} introuvable !" });

            _context.Vehicules.Remove(vehicule);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
