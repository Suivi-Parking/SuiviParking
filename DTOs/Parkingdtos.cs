using System.ComponentModel.DataAnnotations;

namespace Parkini.Application.DTOs;

public class ZoneDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool EstActive { get; set; }
    public int NombrePlaces { get; set; }
    public int PlacesOccupees { get; set; }
    public int PlacesLibres => NombrePlaces - PlacesOccupees;
    public decimal TauxOccupation => NombrePlaces == 0 ? 0 : Math.Round((decimal)PlacesOccupees / NombrePlaces * 100, 1);
}

public class CreateZoneDto
{
    [Required, MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Type { get; set; } = "Standard";

    [Range(1, int.MaxValue, ErrorMessage = "Le nombre de places doit etre superieur a 0.")]
    public int NombrePlaces { get; set; }
}

public class UpdateOccupationZoneDto
{
    [Range(0, int.MaxValue)]
    public int PlacesOccupees { get; set; }
}

public class ParkingDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool Ouvert24h24 { get; set; }
    public TimeOnly? HeureOuverture { get; set; }
    public TimeOnly? HeureFermeture { get; set; }
    public string? ImageUrl { get; set; }
    public string Statut { get; set; } = string.Empty; // "Actif" | "Inactif"
    public DateTime DateDerniereModification { get; set; }

    public int NombreZones { get; set; }

    // Calculee : jamais saisie directement (critere d'acceptation)
    public int CapaciteMax { get; set; }
    public int PlacesOccupees { get; set; }
    public int PlacesLibres => CapaciteMax - PlacesOccupees;
    public decimal TauxOccupation => CapaciteMax == 0 ? 0 : Math.Round((decimal)PlacesOccupees / CapaciteMax * 100, 1);

    // Rempli uniquement sur le GET par id (detail), vide dans la liste
    public List<ZoneDto> Zones { get; set; } = new();
}

public class CreateParkingDto
{
    [Required, MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Adresse { get; set; } = string.Empty;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public bool Ouvert24h24 { get; set; } = false;
    public TimeOnly? HeureOuverture { get; set; }
    public TimeOnly? HeureFermeture { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    // "Actif" | "Inactif" - optionnel, defaut Actif si absent
    public string? Statut { get; set; }

    // Critere d'acceptation : un parking doit avoir au moins une zone des sa creation
    [Required, MinLength(1, ErrorMessage = "Un parking doit avoir au moins une zone.")]
    public List<CreateZoneDto> Zones { get; set; } = new();
}

public class UpdateParkingDto
{
    [Required, MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    [Required, MaxLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Adresse { get; set; } = string.Empty;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public bool Ouvert24h24 { get; set; }
    public TimeOnly? HeureOuverture { get; set; }
    public TimeOnly? HeureFermeture { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public string? Statut { get; set; }
    // Note : la modification des zones se fait via les endpoints /zones dedies,
    // pas via cette mise a jour generale du parking.
}

public class ParkingListeResponseDto
{
    public List<ParkingDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}