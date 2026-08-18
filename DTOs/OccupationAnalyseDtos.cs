namespace Parkini.Application.DTOs;

/// <summary>
/// Requete envoyee au service VLM. C'est le "contrat" avec SCRUM-21 :
/// tant que SCRUM-21 n'est pas termine, on construit cet objet a la main
/// (image de test, zones vides ou factices). Une fois SCRUM-21 livre,
/// il suffira de remplir ImageUrl et Zones avec les vraies donnees,
/// sans toucher au reste du code.
/// </summary>
public class AnalyseOccupationRequest
{
    public int ParkingId { get; set; }

    /// <summary>URL publique de l'image a analyser (viendra de SCRUM-21 a terme).</summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Zones de calibration (polygones normalises 0-1). Null ou vide = pas de
    /// calibration : le VLM analyse l'image entiere comme une seule zone.
    /// </summary>
    public List<ZoneCalibrationDto>? Zones { get; set; }
}

public class ZoneCalibrationDto
{
    public string Nom { get; set; } = string.Empty;

    /// <summary>Points du polygone, normalises entre 0 et 1 (proportion de la largeur/hauteur de l'image).</summary>
    public List<PointDto> Points { get; set; } = new();
}

public class PointDto
{
    public double X { get; set; }
    public double Y { get; set; }
}

/// <summary>Reponse structuree qu'on demande au VLM de produire (format JSON strict dans le prompt).</summary>
public class ResultatAnalyseVlm
{
    public List<ResultatZoneVlm> Zones { get; set; } = new();

    /// <summary>Texte brut renvoye par le VLM, conserve pour debug/audit si le parsing echoue partiellement.</summary>
    public string ReponseBrute { get; set; } = string.Empty;
}

public class ResultatZoneVlm
{
    public string NomZone { get; set; } = string.Empty;
    public int VehiculesDetectes { get; set; }
    public int PlacesLibresEstimees { get; set; }
}
