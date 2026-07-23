

namespace Parkini.Entities;

public class Entreprise
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MatriculeFiscal { get; set; } = string.Empty;
    public string Secteur { get; set; } = string.Empty; // ex: Automobile, Finance, Energie...
    public DateTime DateCreation { get; set; } = DateTime.UtcNow;

    // Soft-delete : on ne supprime jamais vraiment la ligne en base
    public bool EstSupprime { get; set; } = false;

    public ICollection<Employe> Employes { get; set; } = new List<Employe>();
    public ICollection<Abonnement> Abonnements { get; set; } = new List<Abonnement>();
}