namespace Parkini.Entities;

public class Zone
{
    public int Id { get; set; }
    public int ParkingId { get; set; }
    public Parking? Parking { get; set; }

    public string Nom { get; set; } = string.Empty;
    public int NombrePlaces { get; set; } // = "capacite" de la zone dans le vocabulaire du ticket

    // Libre (pas un enum strict) : visiteurs, residents, VIP, livraison, standard...
    // le ticket precise "etc." donc on garde la porte ouverte a de nouveaux types
    public string Type { get; set; } = "Standard";

    public bool EstActive { get; set; } = true;

    // Mis a jour manuellement pour l'instant. Sera pilote automatiquement
    // par le module "Gestion des acces" une fois celui-ci construit.
    public int PlacesOccupees { get; set; } = 0;
}
