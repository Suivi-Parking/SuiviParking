namespace Parkini.Entities;

public enum StatutPaiement
{
    EnAttente,
    Paye,
    Rejete,
    EnRetard
}

public enum ModePaiement
{
    Virement,
    Cheque,
    Carte,
    Prelevement,
    Especes
}

public class Paiement
{
    public int Id { get; set; }
    public int AbonnementId { get; set; }
    public Abonnement? Abonnement { get; set; }

    public decimal Montant { get; set; }
    public DateTime DateEcheance { get; set; }
    public DateTime? DatePaiement { get; set; }
    public ModePaiement? ModePaiement { get; set; }
    public string? ReferenceTransaction { get; set; }
    public string PeriodeCouverte { get; set; } = string.Empty;
    public StatutPaiement Statut { get; set; } = StatutPaiement.EnAttente;
}