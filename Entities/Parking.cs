namespace Parkini.Entities;

public class Parking
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public ICollection<Abonnement> Abonnements { get; set; } = new List<Abonnement>();
}
