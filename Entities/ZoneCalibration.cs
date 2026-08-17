using System.Text.Json;
using System.Text.Json.Serialization;

namespace Parkini.Entities;

/// <summary>
/// Zone de calibration dessinee par l'administrateur sur l'image d'un parking.
/// Reference une vraie Zone (capacite, occupation) plutot qu'un nom libre :
/// ca permet a SCRUM-22 de mettre a jour Zone.PlacesOccupees directement par
/// ZoneId apres l'analyse VLM, sans avoir a faire du rapprochement par texte.
///
/// Les points du polygone sont stockes en JSON (jsonb sous Postgres) : petites
/// listes (4-10 points) toujours lues/ecrites en bloc, pas besoin de table a part.
/// </summary>
public class ZoneCalibration
{
    public int Id { get; set; }

    public int ParkingId { get; set; }
    public Parking? Parking { get; set; }

    public int ZoneId { get; set; }
    public Zone? Zone { get; set; }

    /// <summary>
    /// Points du polygone en JSON, normalises entre 0 et 1 (proportion de la
    /// largeur/hauteur de l'image, independant de la resolution reelle).
    /// </summary>
    public string PointsJson { get; set; } = "[]";

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime DateDerniereModification { get; set; } = DateTime.UtcNow;

    [JsonIgnore]
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public List<PointCalibration> Points
    {
        get => JsonSerializer.Deserialize<List<PointCalibration>>(PointsJson) ?? new();
        set => PointsJson = JsonSerializer.Serialize(value);
    }
}

public class PointCalibration
{
    public double X { get; set; }
    public double Y { get; set; }
}
