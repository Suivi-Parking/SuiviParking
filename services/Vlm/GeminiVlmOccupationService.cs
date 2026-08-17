using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Parkini.Application.DTOs;

namespace Parkini.Infrastructure.Vlm;

/// <summary>
/// Alternative gratuite a OpenAI : l'API Gemini de Google offre un tier gratuit
/// permanent, sans carte bancaire (contrairement a OpenAI qui exige du credit
/// meme pour la cle "sk-proj-..."). Suffisant largement pour du developpement
/// et des tests.
///
/// Gemini ne prend pas d'URL d'image directement dans generateContent : il faut
/// telecharger l'image et l'envoyer encodee en base64 ("inline_data").
/// </summary>
public class GeminiVlmOccupationService : IVlmOccupationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<GeminiVlmOccupationService> _logger;

    public GeminiVlmOccupationService(HttpClient httpClient, IConfiguration config, ILogger<GeminiVlmOccupationService> logger)
    {
        _httpClient = httpClient;
        _config = config;
        _logger = logger;

        _httpClient.BaseAddress ??= new Uri(_config["Vlm:Endpoint"] ?? "https://generativelanguage.googleapis.com/v1beta/");
    }

    public async Task<ResultatAnalyseVlm> AnalyserAsync(AnalyseOccupationRequest requete, CancellationToken ct = default)
    {
        var modele = _config["Vlm:Model"] ?? "gemini-flash-latest";
        // Modele de secours : essaye avec un modele different (souvent moins
        // congestionne, quota separe) si le principal reste indisponible
        // apres toutes les tentatives. Laisse vide dans la config pour desactiver.
        var modeleSecours = _config["Vlm:ModelFallback"] ?? "gemini-2.5-flash-lite";

        // Gemini exige les octets de l'image (base64), pas une URL directement.
        var (imageBytes, mimeType) = await TelechargerImageAsync(requete.ImageUrl, ct);
        var imageBase64 = Convert.ToBase64String(imageBytes);

        var prompt = ConstruirePrompt(requete);

        var corpsRequete = new
        {
            contents = new object[]
            {
                new
                {
                    parts = new object[]
                    {
                        new { text = prompt },
                        new { inline_data = new { mime_type = mimeType, data = imageBase64 } }
                    }
                }
            },
            generationConfig = new { response_mime_type = "application/json" }
        };

        var json = JsonSerializer.Serialize(corpsRequete);

        // Retry avec backoff, puis bascule sur un modele de secours si le
        // modele principal reste indisponible : un 503 "high demand" chez
        // Gemini peut durer plus que quelques secondes sur le tier gratuit,
        // et les modeles ont des pools de capacite/quota distincts, donc
        // changer de modele contourne souvent une surcharge persistante.
        var corpsReponse = await AppelerAvecRetryAsync(modele, json, ct)
            ?? (!string.IsNullOrWhiteSpace(modeleSecours) && modeleSecours != modele
                ? await AppelerAvecRetryAsync(modeleSecours, json, ct)
                : null);

        if (corpsReponse is null)
            throw new InvalidOperationException("Echec de l'appel au VLM (Gemini) : service indisponible apres toutes les tentatives (modele principal et modele de secours). Voir les logs pour le detail.");

        return ParserReponse(corpsReponse);
    }

    /// <summary>
    /// Tente l'appel a un modele donne, avec retry (backoff exponentiel) sur
    /// les erreurs transitoires (503/429). Retourne null si toutes les
    /// tentatives echouent avec une erreur transitoire (pour laisser
    /// AnalyserAsync basculer sur le modele de secours), ou leve directement
    /// si l'erreur n'est pas transitoire (ex: cle invalide, modele inconnu).
    /// </summary>
    private async Task<string?> AppelerAvecRetryAsync(string modele, string json, CancellationToken ct)
    {
        var apiKey = _config["Vlm:ApiKey"]
            ?? throw new InvalidOperationException("Cle API VLM manquante (config 'Vlm:ApiKey').");

        const int tentativesMax = 4;

        for (var tentative = 1; tentative <= tentativesMax; tentative++)
        {
            using var contenu = new StringContent(json, Encoding.UTF8, "application/json");
            using var reponseHttp = await _httpClient.PostAsync(
                $"models/{modele}:generateContent?key={apiKey}", contenu, ct);
            var corpsReponse = await reponseHttp.Content.ReadAsStringAsync(ct);

            if (reponseHttp.IsSuccessStatusCode)
                return corpsReponse;

            var estTransitoire = reponseHttp.StatusCode is System.Net.HttpStatusCode.ServiceUnavailable
                or System.Net.HttpStatusCode.TooManyRequests;

            if (!estTransitoire)
            {
                _logger.LogError("Echec de l'appel Gemini ({Modele}, {Status}) : {Corps}", modele, reponseHttp.StatusCode, corpsReponse);
                throw new InvalidOperationException($"Echec de l'appel au VLM (Gemini, modele {modele}) : {reponseHttp.StatusCode}. Voir les logs pour le detail.");
            }

            if (tentative == tentativesMax)
            {
                _logger.LogWarning("Modele {Modele} indisponible apres {Tentative} tentatives ({Status}).", modele, tentative, reponseHttp.StatusCode);
                return null;
            }

            // Backoff exponentiel plafonne : 1s, 2s, 4s, 8s (+ leger jitter pour eviter
            // que plusieurs requetes en parallele ne retentent toutes exactement au meme instant)
            var delaiBase = Math.Min(Math.Pow(2, tentative - 1), 8);
            var jitterMs = Random.Shared.Next(0, 500);
            var delai = TimeSpan.FromSeconds(delaiBase) + TimeSpan.FromMilliseconds(jitterMs);

            _logger.LogWarning("Gemini indisponible ({Modele}, {Status}), nouvelle tentative dans {Delai:0.0}s ({Tentative}/{Max})",
                modele, reponseHttp.StatusCode, delai.TotalSeconds, tentative, tentativesMax);
            await Task.Delay(delai, ct);
        }

        return null;
    }

    private async Task<(byte[] Bytes, string MimeType)> TelechargerImageAsync(string imageUrl, CancellationToken ct)
    {
        using var reponse = await _httpClient.GetAsync(imageUrl, ct);
        reponse.EnsureSuccessStatusCode();

        var bytes = await reponse.Content.ReadAsByteArrayAsync(ct);
        var mimeType = reponse.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

        return (bytes, mimeType);
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
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
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
            _logger.LogError(ex, "Impossible de parser la reponse de Gemini : {Corps}", corpsReponse);
            throw new InvalidOperationException("Reponse du VLM illisible (voir les logs pour le detail).", ex);
        }
    }
}