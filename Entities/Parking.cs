namespace Parkini.Entities;

public enum StatutParking
{
    Actif,
    Inactif
}

public class Parking
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;

    // Unique dans le systeme (verifie au niveau du controller)
    public string Code { get; set; } = string.Empty;

    public string Adresse { get; set; } = string.Empty;

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    public bool Ouvert24h24 { get; set; } = false;
    public TimeOnly? HeureOuverture { get; set; }
    public TimeOnly? HeureFermeture { get; set; }

    public string? ImageUrl { get; set; }

    public StatutParking Statut { get; set; } = StatutParking.Actif;
    public DateTime DateDerniereModification { get; set; } = DateTime.UtcNow;

    // SCRUM-22 : horodatage dedie a la derniere analyse VLM reussie, distinct
    // de DateDerniereModification (qui bouge aussi sur une simple edition du
    // parking ou de ses zones, et ne reflete donc pas fiablement "l'IA a
    // tourne recemment"). Null tant qu'aucune analyse n'a encore eu lieu.
    public DateTime? DateDerniereAnalyseIa { get; set; }

    // Soft-delete = archivage. Distinct de Statut (Actif/Inactif), qui est un etat
    // metier, alors qu'EstSupprime signifie "n'existe plus du tout" pour l'appli.
    public bool EstSupprime { get; set; } = false;

    public ICollection<Zone> Zones { get; set; } = new List<Zone>();
    public ICollection<Abonnement> Abonnements { get; set; } = new List<Abonnement>();
    public ICollection<HistoriqueParking> Historique { get; set; } = new List<HistoriqueParking>();

    // NOTE : pas de champ CapaciteMax stocke ici volontairement.
    // Critere d'acceptation : "la capacite totale est la somme des capacites des zones"
    // => calculee a la volee (voir ParkingDto.CapaciteMax), jamais saisie a la main,
    // pour ne jamais pouvoir se desynchroniser des zones reelles.
}