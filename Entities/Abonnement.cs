namespace Parkini.Entities;

public enum StatutAbonnement
{
    Actif,
    Suspendu,
    Resilie,
    Expire
}

public class Abonnement
{
    public int Id { get; set; }
    public int EntrepriseId { get; set; }
    public Entreprise? Entreprise { get; set; }

    public int ParkingId { get; set; }
    public Parking? Parking { get; set; }

    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public StatutAbonnement Statut { get; set; } = StatutAbonnement.Actif;
    public decimal MontantMensuel { get; set; }
    public string TypeAbonnement { get; set; } = string.Empty;

    // Renseignee lors d'une suspension ou d'une resiliation
    public string? Raison { get; set; }
}