using System.ComponentModel.DataAnnotations;

namespace Parkini.Application.DTOs;

public class AbonnementDto
{
    public int Id { get; set; }
    public int EntrepriseId { get; set; }
    public string EntrepriseNom { get; set; } = string.Empty;
    public int ParkingId { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public string TypeAbonnement { get; set; } = string.Empty;
    public decimal MontantMensuel { get; set; }
    public string? Raison { get; set; }

    // Statut "effectif" : si DateFin est depassee, affiche "Expire" meme si
    // la colonne en base dit encore "Actif" (pas besoin de tache planifiee).
    public string Statut { get; set; } = string.Empty;
}

public class CreateAbonnementDto
{
    [Required]
    public int EntrepriseId { get; set; }

    [Required]
    public int ParkingId { get; set; }

    [Required]
    public DateTime DateDebut { get; set; }

    [Required]
    public DateTime DateFin { get; set; }

    [Required, MaxLength(50)]
    public string TypeAbonnement { get; set; } = string.Empty;

    [Range(0, double.MaxValue)]
    public decimal MontantMensuel { get; set; }
}

public class RenouvelerAbonnementDto
{
    [Required]
    public DateTime NouvelleDateFin { get; set; }
}

public class ChangerStatutAbonnementDto
{
    [Required, MaxLength(250)]
    public string Raison { get; set; } = string.Empty;
}

public class AbonnementCountsDto
{
    public int Tous { get; set; }
    public int Actif { get; set; }
    public int Suspendu { get; set; }
    public int Resilie { get; set; }
    public int Expire { get; set; }
}

public class AbonnementListeResponseDto
{
    public List<AbonnementDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public AbonnementCountsDto Counts { get; set; } = new();
}

public class StatistiquesMensuellesDto
{
    public int Mois { get; set; } // 1-12
    public string NomMois { get; set; } = string.Empty;
    public int Nouveaux { get; set; }
    public int Actifs { get; set; }
    public int Resiliations { get; set; }
}

public class StatistiquesAnnuellesDto
{
    public int Annee { get; set; }
    public List<StatistiquesMensuellesDto> Mois { get; set; } = new();
}