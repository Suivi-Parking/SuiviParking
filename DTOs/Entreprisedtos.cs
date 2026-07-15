using System.ComponentModel.DataAnnotations;

namespace Parkini.Application.DTOs;

public class EntrepriseDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MatriculeFiscal { get; set; } = string.Empty;
    public string Secteur { get; set; } = string.Empty;
    public DateTime DateCreation { get; set; }
    public int NombreEmployes { get; set; }

    // Calcule a partir de l'abonnement le plus recent : "Actif" | "EnAttente" | "Expire"
    public string StatutAbonnement { get; set; } = "EnAttente";
}

public class CreateEntrepriseDto
{
    [Required, MaxLength(150)]
    public string Nom { get; set; } = string.Empty;

    [Required, MaxLength(250)]
    public string Adresse { get; set; } = string.Empty;

    [Required, Phone]
    public string Telephone { get; set; } = string.Empty;

    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string MatriculeFiscal { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Secteur { get; set; } = string.Empty;
}

public class UpdateEntrepriseDto : CreateEntrepriseDto
{
}

public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

// Reponse specifique a la liste des entreprises : pagination + compteurs pour les pastilles de filtre
public class EntrepriseListeResponseDto
{
    public List<EntrepriseDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public EntrepriseCountsDto Counts { get; set; } = new();
}

public class EntrepriseCountsDto
{
    public int Tous { get; set; }
    public int Actif { get; set; }
    public int EnAttente { get; set; }
    public int Expire { get; set; }
}