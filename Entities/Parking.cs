namespace Parkini.Entities;

public class Parking
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public int CapaciteMax { get; set; }

    // Soft-delete, meme convention que pour Entreprise
    public bool EstSupprime { get; set; } = false;

    public ICollection<Zone> Zones { get; set; } = new List<Zone>();
    public ICollection<Abonnement> Abonnements { get; set; } = new List<Abonnement>();
}