using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Parkini.Infrastructure.Data;
using Parkini.Application.DTOs; 
using Parkini.Entities;

namespace Parkini.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AccesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // POST api/acces/entree
        // Autorise (ou refuse) l'entrée d'un véhicule dans un parking
        // =========================================================
        [HttpPost("entree")]
        public async Task<ActionResult<AccesDto>> AutoriserEntree(DemandeAccesDto dto)
        {
            var vehicule = await _context.Vehicules
                .Include(v => v.Employe)
                .FirstOrDefaultAsync(v => v.Id == dto.VehiculeId);

            if (vehicule == null)
                return NotFound("Véhicule introuvable.");

            var parking = await _context.Parkings
                .FirstOrDefaultAsync(p => p.Id == dto.ParkingId);

            if (parking == null)
                return NotFound("Parking introuvable.");

            // Empêche une double entrée pour un véhicule déjà "EN_COURS"
            var accesEnCours = await _context.Acces
                .AnyAsync(a => a.VehiculeId == dto.VehiculeId && a.Statut == "EN_COURS");

            if (accesEnCours)
                return BadRequest("Ce véhicule a déjà un accès en cours (pas encore sorti).");

            // Vérification de l'abonnement (si fourni)
            string? motifRefus = null;
            bool estAutorise = true;

            if (dto.AbonnementId.HasValue)
            {
                var abonnementValide = await VerifierValiditeAbonnementInterne(dto.AbonnementId.Value);
                if (!abonnementValide)
                {
                    estAutorise = false;
                    motifRefus = "Abonnement invalide, expiré ou inactif.";
                }
            }
            else
            {
                // Pas d'abonnement fourni : refuse par défaut
                // (à adapter si des accès ponctuels/payants à l'unité sont prévus)
                estAutorise = false;
                motifRefus = "Aucun abonnement associé à la demande d'accès.";
            }

            // Vérification de la capacité du parking (si la propriété existe côté entité)
            // À adapter selon vos champs réels (ex: parking.CapaciteMax, parking.PlacesDisponibles)

            var acces = new Acces
            {
                VehiculeId = dto.VehiculeId,
                ParkingId = dto.ParkingId,
                AbonnementId = dto.AbonnementId,
                DateHeureEntree = DateTime.UtcNow,
                EstAutorise = estAutorise,
                MotifRefus = motifRefus,
                Statut = estAutorise ? "EN_COURS" : "REFUSE"
            };

            _context.Acces.Add(acces);
            await _context.SaveChangesAsync();

            var resultDto = MapToDto(acces, vehicule);

            if (!estAutorise)
                return BadRequest(new { message = motifRefus, acces = resultDto });

            return CreatedAtAction(nameof(GetById), new { id = acces.Id }, resultDto);
        }

        // =========================================================
        // POST api/acces/sortie
        // Enregistre la sortie d'un véhicule
        // =========================================================
        [HttpPost("sortie")]
        public async Task<ActionResult<AccesDto>> EnregistrerSortie(SortieAccesDto dto)
        {
            var acces = await _context.Acces
                .Include(a => a.Vehicule)
                .ThenInclude(v => v!.Employe)
                .FirstOrDefaultAsync(a => a.Id == dto.AccesId);

            if (acces == null)
                return NotFound("Accès introuvable.");

            if (acces.Statut != "EN_COURS")
                return BadRequest("Cet accès n'est pas en cours (déjà sorti ou refusé).");

            acces.DateHeureSortie = DateTime.UtcNow;
            acces.Statut = "SORTI";

            await _context.SaveChangesAsync();

            return Ok(MapToDto(acces, acces.Vehicule));
        }

        // =========================================================
        // GET api/acces/verifier-abonnement/{abonnementId}
        // Vérifie la validité d'un abonnement
        // =========================================================
        [HttpGet("verifier-abonnement/{abonnementId:int}")]
        public async Task<ActionResult<bool>> VerifierValiditeAbonnement(int abonnementId)
        {
            var valide = await VerifierValiditeAbonnementInterne(abonnementId);
            return Ok(new { abonnementId, estValide = valide });
        }

        private async Task<bool> VerifierValiditeAbonnementInterne(int abonnementId)
        {
            var abonnement = await _context.Abonnements
                .FirstOrDefaultAsync(a => a.Id == abonnementId);

            if (abonnement == null)
                return false;

            // NOTE : adapter ces conditions aux champs réels de l'entité Abonnement
            // (ex : abonnement.Statut, abonnement.DateDebut, abonnement.DateFin)
            var statutActif = abonnement.Statut.ToString() == "Actif";
            var dateValide = abonnement.DateFin >= DateTime.UtcNow;

            return statutActif && dateValide;
        }

        // =========================================================
        // GET api/acces  (historique paginé, avec filtres optionnels)
        // =========================================================
        [HttpGet]
        public async Task<ActionResult<AccesListeResponseDto>> GetHistorique(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int? vehiculeId = null,
            [FromQuery] int? parkingId = null,
            [FromQuery] string? statut = null,
            [FromQuery] DateTime? dateDebut = null,
            [FromQuery] DateTime? dateFin = null)
        {
            var query = _context.Acces
                .Include(a => a.Vehicule)
                .ThenInclude(v => v!.Employe)
                .AsQueryable();

            if (vehiculeId.HasValue)
                query = query.Where(a => a.VehiculeId == vehiculeId.Value);

            if (parkingId.HasValue)
                query = query.Where(a => a.ParkingId == parkingId.Value);

            if (!string.IsNullOrEmpty(statut))
                query = query.Where(a => a.Statut == statut);

            if (dateDebut.HasValue)
                query = query.Where(a => a.DateHeureEntree >= dateDebut.Value);

            if (dateFin.HasValue)
                query = query.Where(a => a.DateHeureEntree <= dateFin.Value);

            query = query.OrderByDescending(a => a.DateHeureEntree);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = new AccesListeResponseDto
            {
                Items = items.Select(a => MapToDto(a, a.Vehicule)).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Ok(response);
        }

        // =========================================================
        // GET api/acces/{id}
        // =========================================================
        [HttpGet("{id:int}")]
        public async Task<ActionResult<AccesDto>> GetById(int id)
        {
            var acces = await _context.Acces
                .Include(a => a.Vehicule)
                .ThenInclude(v => v!.Employe)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (acces == null)
                return NotFound();

            return Ok(MapToDto(acces, acces.Vehicule));
        }

        // =========================================================
        // Mapping Entity -> DTO
        // =========================================================
        private static AccesDto MapToDto(Acces acces, Vehicule? vehicule)
        {
            var duree = acces.CalculerDuree();

            return new AccesDto
            {
                Id = acces.Id,
                DateHeureEntree = acces.DateHeureEntree,
                DateHeureSortie = acces.DateHeureSortie,
                EstAutorise = acces.EstAutorise,
                MotifRefus = acces.MotifRefus,
                Statut = acces.Statut,
                VehiculeId = acces.VehiculeId,
                VehiculeImmatriculation = vehicule?.Immatriculation ?? string.Empty,
                EmployeNom = vehicule?.Employe?.Nom ?? string.Empty,
                ParkingId = acces.ParkingId,
                AbonnementId = acces.AbonnementId,
                DureeFormatee = duree.HasValue
                    ? $"{(int)duree.Value.TotalHours:D2}h{duree.Value.Minutes:D2}"
                    : null
            };
        }
    }
}