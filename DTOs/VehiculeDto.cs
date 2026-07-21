using System.ComponentModel.DataAnnotations;

namespace Parkini.Application.DTOs;
// DTO pour afficher un véhicule
public class VehiculeDto
{
    public int Id { get; set; }
    public string Immatriculation { get; set; } = string.Empty;
    public string Marque { get; set; } = string.Empty;
    public string Modele { get; set; } = string.Empty;
    public string Couleur { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Statut { get; set; } = string.Empty;
    public int EmployeId { get; set; }
    public string EmployeNom { get; set; } = string.Empty;
}

// DTO pour créer un véhicule
public class CreateVehiculeDto
{
    [Required]
    [MaxLength(20)]
    public string Immatriculation { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Marque { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Modele { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Couleur { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Type { get; set; } = string.Empty;

    [Required]
    public int EmployeId { get; set; }
}

// DTO pour modifier un véhicule
public class UpdateVehiculeDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Immatriculation { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Marque { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Modele { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Couleur { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Statut { get; set; } = string.Empty;

    [Required]
    public int EmployeId { get; set; }
}

// DTO pour la liste avec pagination
public class VehiculeListeResponseDto
{
    public List<VehiculeDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}