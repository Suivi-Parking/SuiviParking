using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Parkini.Application.DTOs;
using Parkini.Infrastructure.Security;
using Parkini.Infrastructure.Vlm;

namespace Parkini.Controllers;

/// <summary>
/// Declenchement manuel de l'analyse VLM (utile pour tester un parking
/// precis sans attendre le prochain passage du job planifie).
/// La logique elle-meme vit dans OccupationAnalyseService, partagee avec
/// OccupationJobs (declenchement automatique, qui lui n'a pas besoin
/// d'authentification puisqu'il tourne cote serveur, pas depuis un navigateur).
///
/// Reutilise Permissions.ParkingsWrite plutot qu'une permission dediee :
/// declencher une analyse modifie l'occupation d'un parking, exactement comme
/// les autres actions d'ecriture sur les parkings deja soumises a cette
/// permission dans ParkingsController.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OccupationAnalyseController : ControllerBase
{
    private readonly IOccupationAnalyseService _analyseService;

    public OccupationAnalyseController(IOccupationAnalyseService analyseService)
    {
        _analyseService = analyseService;
    }

    // POST api/OccupationAnalyse/analyser/5?imageUrlTest=https://...
    [HasPermission(Permissions.ParkingsWrite)]
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