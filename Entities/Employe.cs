namespace Parkini.Entities;

public class Employe
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telephone { get; set; } = string.Empty;
    public string Fonction { get; set; } = string.Empty;
    public string Statut { get; set; } = "ACTIF";
    public DateTime DateRattachement { get; set; } = DateTime.UtcNow;
    public DateTime DateDerniereModification { get; set; } = DateTime.UtcNow;
    public int EntrepriseId { get; set; }
    public ICollection<Vehicule> Vehicules { get; set; } = new List<Vehicule>();
}