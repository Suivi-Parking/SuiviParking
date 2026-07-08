using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviParking.API.Data;
using SuiviParking.API.Models;

namespace SuiviParking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiculeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VehiculeController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/vehicule
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehicule>>> GetAll()
        {
            return await _context.Vehicules
                .Include(v => v.Employe)
                .ToListAsync();
        }

        // GET api/vehicule/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Vehicule>> GetById(int id)
        {
            var vehicule = await _context.Vehicules
                .Include(v => v.Employe)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicule == null)
                return NotFound();

            return vehicule;
        }

        // GET api/vehicule/employe/3
        [HttpGet("employe/{employeId}")]
        public async Task<ActionResult<IEnumerable<Vehicule>>> GetByEmploye(int employeId)
        {
            return await _context.Vehicules
                .Where(v => v.EmployeId == employeId)
                .ToListAsync();
        }

        // POST api/vehicule
        [HttpPost]
        public async Task<ActionResult<Vehicule>> Create(Vehicule vehicule)
        {
            // Vérifier immatriculation unique
            if (await _context.Vehicules
                .AnyAsync(v => v.Immatriculation == vehicule.Immatriculation))
                return BadRequest(new { message = "Cette immatriculation existe déjà !" });

            // Vérifier que l'employé existe
            if (!await _context.Employes.AnyAsync(e => e.Id == vehicule.EmployeId))
                return BadRequest(new { message = "Employé introuvable !" });

            _context.Vehicules.Add(vehicule);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = vehicule.Id }, vehicule);
        }

        // PUT api/vehicule/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Vehicule vehicule)
        {
            if (id != vehicule.Id)
                return BadRequest();

            // Vérifier immatriculation unique
            if (await _context.Vehicules
                .AnyAsync(v => v.Immatriculation == vehicule.Immatriculation
                && v.Id != id))
                return BadRequest(new { message = "Cette immatriculation existe déjà !" });

            _context.Entry(vehicule).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Vehicules.AnyAsync(v => v.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE api/vehicule/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vehicule = await _context.Vehicules.FindAsync(id);
            if (vehicule == null)
                return NotFound();

            _context.Vehicules.Remove(vehicule);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}