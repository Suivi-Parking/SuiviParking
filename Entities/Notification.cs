namespace Parkini.Entities;

public enum TypeNotification
{
    RenouvellementAbonnement,
    RappelPaiement,
    AccesRefuse,
    SaturationParking,
    MiseAJourInfos
}

public enum CanalNotification
{
    Email,
    InApp,
    Sms
}

public enum StatutNotification
{
    EnAttente,
    Envoyee,
    Echouee
}

public class Notification
{
    public int Id { get; set; }
    public TypeNotification Type { get; set; }
    public CanalNotification Canal { get; set; } = CanalNotification.Email;
    public StatutNotification Statut { get; set; } = StatutNotification.EnAttente;

    public string Destinataire { get; set; } = string.Empty;
    public string Sujet { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public int? EntrepriseId { get; set; }
    public Entreprise? Entreprise { get; set; }

    public int? AbonnementId { get; set; }
    public Abonnement? Abonnement { get; set; }

    public int? ParkingId { get; set; }
    public Parking? Parking { get; set; }

    public int? AccesId { get; set; }
    public Acces? Acces { get; set; }

    public int? PaiementId { get; set; }
    public Paiement? Paiement { get; set; }

    public int? EmployeId { get; set; }
    public Employe? Employe { get; set; }

    public int? VehiculeId { get; set; }
    public Vehicule? Vehicule { get; set; }

    public DateTime DateCreation { get; set; } = DateTime.UtcNow;
    public DateTime? DateEnvoi { get; set; }
    public string? ErreurDetail { get; set; }
}