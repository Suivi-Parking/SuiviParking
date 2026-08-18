using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Parkini.Infrastructure.Data;

namespace Parkini.Infrastructure.Vlm;

/// <summary>
/// Job planifie (Hangfire) qui declenche l'analyse VLM pour tous les parkings
/// ayant une image configuree. Meme principe que RappelsJobs pour les
/// notifications : une classe scoped, enregistree en RecurringJob dans
/// Program.cs.
///
/// Chaque parking est traite independamment : si l'un echoue (VLM injoignable,
/// image invalide...), les autres continuent d'etre traites. Les echecs sont
/// logges mais n'interrompent jamais le job entier.
/// </summary>
public class OccupationJobs
{
    private readonly ApplicationDbContext _context;
    private readonly IOccupationAnalyseService _analyseService;
    private readonly ILogger<OccupationJobs> _logger;

    public OccupationJobs(ApplicationDbContext context, IOccupationAnalyseService analyseService, ILogger<OccupationJobs> logger)
    {
        _context = context;
        _analyseService = analyseService;
        _logger = logger;
    }

    public async Task AnalyserTousLesParkings()
    {
        var parkingIds = await _context.Parkings
            .Where(p => p.ImageUrl != null && p.ImageUrl != "")
            .Select(p => p.Id)
            .ToListAsync();

        _logger.LogInformation("Analyse VLM planifiee : {Nombre} parking(s) a traiter.", parkingIds.Count);

        var succes = 0;
        var echecs = 0;

        foreach (var parkingId in parkingIds)
        {
            try
            {
                await _analyseService.AnalyserParkingAsync(parkingId);
                succes++;
            }
            catch (Exception ex)
            {
                echecs++;
                _logger.LogError(ex, "Echec de l'analyse VLM planifiee pour le parking {ParkingId}.", parkingId);
                // On continue avec le parking suivant plutot que d'interrompre tout le job.
            }
        }

        _logger.LogInformation("Analyse VLM planifiee terminee : {Succes} succes, {Echecs} echec(s).", succes, echecs);
    }
}
