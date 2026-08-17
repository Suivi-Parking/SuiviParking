namespace Parkini.Application.DTOs;

// NOTE : PointDto est deja defini dans OccupationAnalyseDtos.cs (utilise par le
// service VLM de SCRUM-22). On le reutilise ici volontairement, meme contrat
// de coordonnees normalisees 0-1.

/// <summary>Zone de calibration telle que renvoyee par l'API (lecture).</summary>
public class CalibrationZoneDto
{
    public int Id { get; set; }
    public int ParkingId { get; set; }
    public int ZoneId { get; set; }
    public string NomZone { get; set; } = string.Empty; // vient de Zone.Nom, pas saisi ici
    public List<PointDto> Points { get; set; } = new();
    public DateTime DateCreation { get; set; }
    public DateTime DateDerniereModification { get; set; }
}

/// <summary>Une zone envoyee par l'UI admin lors de l'enregistrement (ecriture).</summary>
public class CalibrationZoneInputDto
{
    /// <summary>Id d'une Zone existante du parking (voir GET /api/Parkings/{id} pour la liste).</summary>
    public int ZoneId { get; set; }
    public List<PointDto> Points { get; set; } = new();
}
