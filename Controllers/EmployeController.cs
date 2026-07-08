using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuiviParking.API.Data;
using SuiviParking.API.Models;

namespace SuiviParking.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmployeController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/employe
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employe>>> GetAll()
        {
            return await _context.Employes
                .Include(e => e.Vehicules)
                .ToListAsync();
        }

        // GET api/employe/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employe>> GetById(int id)
        {
            var employe = await _context.Employes
                .Include(e => e.Vehicules)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employe == null)
                return NotFound();

            return employe;
        }

        // GET api/employe/entreprise/3
        [HttpGet("entreprise/{entrepriseId}")]
        public async Task<ActionResult<IEnumerable<Employe>>> GetByEntreprise(int entrepriseId)
        {
            return await _context.Employes
                .Where(e => e.EntrepriseId == entrepriseId)
                .Include(e => e.Vehicules)
                .ToListAsync();
        }

        // POST api/employe
        [HttpPost]
        public async Task<ActionResult<Employe>> Create(Employe employe)
        {
            // Vérifier email unique
            if (await _context.Employes.AnyAsync(e => e.Email == employe.Email))
                return BadRequest(new { message = "Cet email existe déjà !" });

            _context.Employes.Add(employe);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById),
                new { id = employe.Id }, employe);
        }

        // PUT api/employe/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Employe employe)
        {
            if (id != employe.Id)
                return BadRequest();

            // Vérifier email unique
            if (await _context.Employes
                .AnyAsync(e => e.Email == employe.Email && e.Id != id))
                return BadRequest(new { message = "Cet email existe déjà !" });

            _context.Entry(employe).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Employes.AnyAsync(e => e.Id == id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE api/employe/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var employe = await _context.Employes.FindAsync(id);
            if (employe == null)
                return NotFound();

            _context.Employes.Remove(employe);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}