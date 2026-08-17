using Microsoft.AspNetCore.Mvc;
using Parkini.Application.DTOs;
using Parkini.Infrastructure.Vlm;

namespace Parkini.Controllers;

/// <summary>
/// Declenchement manuel de l'analyse VLM (utile pour tester un parking
/// precis sans attendre le prochain passage du job planifie).
/// La logique elle-meme vit dans OccupationAnalyseService, partagee avec
/// OccupationJobs (declenchement automatique).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OccupationAnalyseController : ControllerBase
{
    private readonly IOccupationAnalyseService _analyseService;

    public OccupationAnalyseController(IOccupationAnalyseService analyseService)
    {
        _analyseService = analyseService;
    }

    // POST api/OccupationAnalyse/analyser/5?imageUrlTest=https://...
    [HttpPost("analyser/{parkingId:int}")]
    public async Task<ActionResult<ResultatAnalyseVlm>> Analyser(int parkingId, [FromQuery] string? imageUrlTest = null)
    {
        try
        {
            var resultat = await _analyseService.AnalyserParkingAsync(parkingId, imageUrlTest);
            return Ok(resultat);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("introuvable"))
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(502, new { message = "Le VLM n'a pas pu etre contacte ou sa reponse est illisible.", detail = ex.Message });
        }
    }
}