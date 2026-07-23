namespace Parkini.Application.DTOs;

public class PaiementDto
{
    public int Id { get; set; }
    public int AbonnementId { get; set; }
    public string EntrepriseNom { get; set; } = string.Empty;
    public string TypeAbonnement { get; set; } = string.Empty;
    public decimal Montant { get; set; }
    public DateTime DateEcheance { get; set; }
    public DateTime? DatePaiement { get; set; }
    public string? ModePaiement { get; set; }
    public string? ReferenceTransaction { get; set; }
    public string PeriodeCouverte { get; set; } = string.Empty;
    public string Statut { get; set; } = string.Empty;
    public int JoursRetard { get; set; }
}

public class CreatePaiementDto
{
    public int AbonnementId { get; set; }
    public decimal Montant { get; set; }
    public DateTime DateEcheance { get; set; }
    public string PeriodeCouverte { get; set; } = string.Empty;
}

public class MarquerPayeDto
{
    public DateTime DatePaiement { get; set; }
    public string ModePaiement { get; set; } = string.Empty; // "Virement" | "Cheque" | "Carte" | "Prelevement" | "Especes"
    public string? ReferenceTransaction { get; set; }
}

public class RejeterPaiementDto
{
    public string? Motif { get; set; }
}

public class PaiementCountsDto
{
    public int Tous { get; set; }
    public int Paye { get; set; }
    public int EnAttente { get; set; }
    public int EnRetard { get; set; }
    public int Rejete { get; set; }
}

public class PaiementListeResponseDto
{
    public List<PaiementDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public PaiementCountsDto Counts { get; set; } = new();
}

public class IndicateursFinanciersDto
{
    public decimal MontantTotalPercu { get; set; }
    public decimal MontantImpaye { get; set; }
    public double TauxRecouvrement { get; set; } // en %
    public Dictionary<string, decimal> RepartitionParModePaiement { get; set; } = new();
}