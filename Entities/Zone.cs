namespace Parkini.Entities;

public class Zone
{
    public int Id { get; set; }
    public int ParkingId { get; set; }
    public Parking? Parking { get; set; }

    public string Nom { get; set; } = string.Empty;
    public int NombrePlaces { get; set; }

    // Mis a jour manuellement pour l'instant. Sera pilote automatiquement
    // par le module "Gestion des acces" une fois celui-ci construit.
    public int PlacesOccupees { get; set; } = 0;
}