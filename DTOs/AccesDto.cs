using System.ComponentModel.DataAnnotations;

namespace Parkini.Application.DTOs;

// DTO pour afficher un accès0.

public class AccesDto
{
    public int Id { get; set; }
    public DateTime DateHeureEntree { get; set; }
    public DateTime? DateHeureSortie { get; set; }
    public bool EstAutorise { get; set; }
    public string? MotifRefus { get; set; }
    public string Statut { get; set; } = string.Empty;
    public int VehiculeId { get; set; }
    public string VehiculeImmatriculation { get; set; } = string.Empty;
    public string EmployeNom { get; set; } = string.Empty;
    public int ParkingId { get; set; }
    public int? AbonnementId { get; set; }
    public string? DureeFormatee { get; set; }
}
 
// DTO pour demander un accès (entrée)
public class DemandeAccesDto
{
    [Required]
    public int VehiculeId { get; set; }

    [Required]
    public int ParkingId { get; set; }

    public int? AbonnementId { get; set; }
}

// DTO pour enregistrer la sortie
public class SortieAccesDto
{
    [Required]
    public int AccesId { get; set; }
}

// DTO pour la liste avec pagination
public class AccesListeResponseDto
{
    public List<AccesDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}