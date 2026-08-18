using Parkini.Application.DTOs;

namespace Parkini.Infrastructure.Vlm;

public interface IVlmOccupationService
{
    Task<ResultatAnalyseVlm> AnalyserAsync(AnalyseOccupationRequest requete, CancellationToken ct = default);
}
