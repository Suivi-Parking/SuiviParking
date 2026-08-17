using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Parkini.Application.DTOs;

namespace Parkini.Infrastructure.Vlm;

/// <summary>
/// Appelle GPT-4o (endpoint /chat/completions avec image_url) pour compter
/// vehicules et places libres. Le prompt demande explicitement du JSON strict
/// pour eviter d'avoir a parser du texte libre, ce qui est fragile.
/// </summary>
public class OpenAiVlmOccupationService : IVlmOccupationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<OpenAiVlmOccupationService> _logger;

    public OpenAiVlmOccupationService(HttpClient httpClient, IConfiguration config, ILogger<OpenAiVlmOccupationService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;

        var apiKey = _config["Vlm:ApiKey"]
            ?? throw new InvalidOperationException("Cle API VLM manquante (config 'Vlm:ApiKey' dans appsettings ou secrets utilisateur).");

        _httpClient.BaseAddress ??= new Uri(_config["Vlm:Endpoint"] ?? "https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<ResultatAnalyseVlm> AnalyserAsync(AnalyseOccupationRequest requete, CancellationToken ct = default)
    {
        var prompt = ConstruirePrompt(requete);

        var corpsRequete = new
        {
            model = _config["Vlm:Model"] ?? "gpt-4o",
            response_format = new { type = "json_object" },
            messages = new object[]
            {
                new
                {
                    role = "system",
                    content = "Tu es un systeme de vision qui compte des vehicules sur des images de parking. " +
                              "Reponds UNIQUEMENT avec du JSON valide, sans aucun texte autour."
                },
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = prompt },
                        new { type = "image_url", image_url = new { url = requete.ImageUrl } }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(corpsRequete);
        using var contenu = new StringContent(json, Encoding.UTF8, "application/json");

        using var reponseHttp = await _httpClient.PostAsync("chat/completions", contenu, ct);
        var corpsReponse = await reponseHttp.Content.ReadAsStringAsync(ct);

        if (!reponseHttp.IsSuccessStatusCode)
        {
            _logger.LogError("Echec de l'appel VLM ({Status}) : {Corps}", reponseHttp.StatusCode, corpsReponse);
            throw new InvalidOperationException($"Echec de l'appel au VLM : {reponseHttp.StatusCode}. Voir les logs pour le detail.");
        }

        return ParserReponse(corpsReponse);
    }

    private static string ConstruirePrompt(AnalyseOccupationRequest requete)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Analyse cette image de parking et compte, pour chaque zone decrite ci-dessous, ")
          .AppendLine("le nombre de vehicules gares et le nombre de places visiblement libres.");
        sb.AppendLine();

        if (requete.Zones is { Count: > 0 })
        {
            sb.AppendLine("Zones a analyser (coordonnees normalisees entre 0 et 1, origine en haut a gauche de l'image) :");
            foreach (var zone in requete.Zones)
            {
                var points = string.Join(", ", zone.Points.Select(p => $"({p.X:0.00},{p.Y:0.00})"));
                sb.AppendLine($"- \"{zone.Nom}\" : polygone [{points}]");
            }
        }
        else
        {
            sb.AppendLine("Aucune zone de calibration fournie : traite l'image entiere comme une seule zone nommee \"image_entiere\".");
        }

        sb.AppendLine();
        sb.AppendLine("Reponds STRICTEMENT avec ce format JSON (aucun texte hors JSON, aucune explication) :");
        sb.AppendLine("{\"zones\": [{\"nomZone\": \"string\", \"vehiculesDetectes\": 0, \"placesLibresEstimees\": 0}]}");

        return sb.ToString();
    }

    private ResultatAnalyseVlm ParserReponse(string corpsReponse)
    {
        try
        {
            using var enveloppe = JsonDocument.Parse(corpsReponse);
            var contenuTexte = enveloppe.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString() ?? "{}";

            using var payload = JsonDocument.Parse(contenuTexte);
            var resultat = new ResultatAnalyseVlm { ReponseBrute = contenuTexte };

            if (payload.RootElement.TryGetProperty("zones", out var zonesElement))
            {
                foreach (var z in zonesElement.EnumerateArray())
                {
                    resultat.Zones.Add(new ResultatZoneVlm
                    {
                        NomZone = z.TryGetProperty("nomZone", out var n) ? n.GetString() ?? "inconnue" : "inconnue",
                        VehiculesDetectes = z.TryGetProperty("vehiculesDetectes", out var v) ? v.GetInt32() : 0,
                        PlacesLibresEstimees = z.TryGetProperty("placesLibresEstimees", out var pl) ? pl.GetInt32() : 0
                    });
                }
            }

            return resultat;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Impossible de parser la reponse du VLM : {Corps}", corpsReponse);
            throw new InvalidOperationException("Reponse du VLM illisible (voir les logs pour le detail).", ex);
        }
    }
}
