using Microsoft.EntityFrameworkCore;
using Parkini.Infrastructure.Data;

namespace Parkini.Infrastructure.Calibration;

public interface IImageParkingService
{
    Task<(byte[] Bytes, string MimeType)> RecupererImageAsync(int parkingId, CancellationToken ct = default);
}

/// <summary>
/// Recupere l'image d'un parking via Parking.ImageUrl (URL de camera/webcam,
/// supposee retourner un snapshot JPEG/PNG frais a chaque appel GET).
///
/// HYPOTHESE A VALIDER : si l'API externe fournie par l'enonce du ticket
/// necessite une cle API, des headers particuliers, ou un format de reponse
/// different (ex: JSON encapsulant une image base64), adapter cette classe --
/// le reste du systeme (calibration, VLM) n'a besoin que de (byte[], mimeType)
/// et n'a pas a changer.
/// </summary>
public class ImageParkingService : IImageParkingService
{
    private readonly HttpClient _httpClient;
    private readonly ApplicationDbContext _context;

    public ImageParkingService(HttpClient httpClient, ApplicationDbContext context)
    {
        _httpClient = httpClient;
        _context = context;
    }

    public async Task<(byte[] Bytes, string MimeType)> RecupererImageAsync(int parkingId, CancellationToken ct = default)
    {
        var parking = await _context.Parkings
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == parkingId, ct);

        if (parking is null)
            throw new InvalidOperationException($"Parking {parkingId} introuvable.");

        if (string.IsNullOrWhiteSpace(parking.ImageUrl))
            throw new InvalidOperationException($"Aucune URL d'image (camera) configuree pour le parking {parkingId}.");

        using var reponse = await _httpClient.GetAsync(parking.ImageUrl, ct);
        reponse.EnsureSuccessStatusCode();

        var bytes = await reponse.Content.ReadAsByteArrayAsync(ct);
        var mimeType = reponse.Content.Headers.ContentType?.MediaType ?? "image/jpeg";

        return (bytes, mimeType);
    }
}
