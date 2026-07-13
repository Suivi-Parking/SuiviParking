using System.ComponentModel.DataAnnotations;

namespace Parkini.Application.DTOs;

public class ZoneDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public int NombrePlaces { get; set; }
    public int PlacesOccupees { get; set; }
    public int PlacesLibres => NombrePlaces - PlacesOccupees;
    public decimal TauxOccupation => NombrePlaces == 0 ? 0 : Math.Round((decimal)PlacesOccupees / NombrePlaces * 100, 1);
}

public class CreateZoneDto
{
    [Required, MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

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
    public string Adresse { get; set; } = string.Empty;
    public int CapaciteMax { get; set; }

    public int NombreZones { get; set; }
    public int PlacesTotales { get; set; }
    public int PlacesOccupees { get; set; }
    public int PlacesLibres => PlacesTotales - PlacesOccupees;
    public decimal TauxOccupation => PlacesTotales == 0 ? 0 : Math.Round((decimal)PlacesOccupees / PlacesTotales * 100, 1);

    // Rempli uniquement sur le GET par id (detail), vide dans la liste
    public List<ZoneDto> Zones { get; set; } = new();
}

public class CreateParkingDto
{
    [Required, MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Adresse { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int CapaciteMax { get; set; }
}

public class UpdateParkingDto : CreateParkingDto
{
}

public class ParkingListeResponseDto
{
    public List<ParkingDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}