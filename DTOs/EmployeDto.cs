using System.ComponentModel.DataAnnotations;


namespace Parkini.Application.DTOs;


// DTO pour afficher un employé
public class EmployeDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Fonction { get; set; } = string.Empty;
    public string Statut { get; set; } = string.Empty;
    public DateTime DateRattachement { get; set; }
    public int EntrepriseId { get; set; }
    public List<VehiculeDto> Vehicules { get; set; } = new();
}

// DTO pour créer un employé
public class CreateEmployeDto
{
    [Required]
    [MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Prenom { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Telephone { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Fonction { get; set; } = string.Empty;

    [Required]
    public int EntrepriseId { get; set; }
}

// DTO pour modifier un employé
public class UpdateEmployeDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nom { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Prenom { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Telephone { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Fonction { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Statut { get; set; } = string.Empty;

    [Required]
    public int EntrepriseId { get; set; }
}

// DTO pour la liste avec pagination
public class EmployeListeResponseDto
{
    public List<EmployeDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}