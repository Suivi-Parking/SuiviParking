namespace Parkini.Application.DTOs;

public class DashboardDto
{
    // ===== Entreprises =====
    public int TotalEntreprises { get; set; }
    public int EntreprisesActives { get; set; }
    public int EntreprisesInactives { get; set; }

    // ===== Abonnements =====
    public int AbonnementsActifs { get; set; }
    public int AbonnementsExpires { get; set; }
    public int AbonnementsExpirantSous30Jours { get; set; }
    public List<RepartitionAbonnementDto> RepartitionAbonnements { get; set; } = new();

    public int TotalEmployes { get; set; }
    public int TotalVehicules { get; set; }

    // ===== Parkings =====
    public int TotalParkings { get; set; }
    public double TauxOccupationGlobal { get; set; }
    public ParkingResumeDto? ParkingLePlusOccupe { get; set; }
    public ParkingResumeDto? ParkingLeMoinsOccupe { get; set; }
    public List<ParkingOccupationDto> OccupationParParking { get; set; } = new();
    public List<AlerteSaturationDto> AlertesSaturation { get; set; } = new();

    // ===== Finances =====
    public decimal RevenuTotal { get; set; }
    public decimal RevenuMensuel { get; set; }
    public decimal MontantImpaye { get; set; }
    public int NombrePaiementsEnRetard { get; set; }
    public List<RevenuEntrepriseDto> RevenuParEntreprise { get; set; } = new();
    public List<RevenuMensuelDto> RevenusParMois { get; set; } = new();

    // ===== Notifications =====
    // Reutilise le NotificationDto existant (NotificationDtos.cs)
    public List<NotificationDto> DernieresNotifications { get; set; } = new();
}

/// <summary>Résumé pour "parking le plus / le moins occupé".</summary>
public class ParkingResumeDto
{
    public int ParkingId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public double TauxOccupation { get; set; }
}

/// <summary>Une ligne du graphique "occupation par parking" (Chart.js, code couleur).</summary>
public class ParkingOccupationDto
{
    public int ParkingId { get; set; }
    public string Nom { get; set; } = string.Empty;
    public int NombrePlaces { get; set; }
    public int PlacesOccupees { get; set; }
    public double TauxOccupation { get; set; }

    /// <summary>"vert" (&lt;60%), "jaune" (60-85%), "rouge" (&gt;85%).</summary>
    public string Statut { get; set; } = "vert";
}

public class AlerteSaturationDto
{
    public int ParkingId { get; set; }
    public string NomParking { get; set; } = string.Empty;
    public double TauxOccupation { get; set; }
    public string Niveau { get; set; } = "critique"; // "critique" (>=90%) ou "eleve" (>=85%)
}

public class RepartitionAbonnementDto
{
    public string Type { get; set; } = string.Empty; // valeur libre d'Abonnement.TypeAbonnement
    public int Nombre { get; set; }
}

public class RevenuEntrepriseDto
{
    public int EntrepriseId { get; set; }
    public string NomEntreprise { get; set; } = string.Empty;
    public decimal RevenuTotal { get; set; }
    public decimal RevenuMensuel { get; set; }
    public decimal MontantImpaye { get; set; }
}

public class RevenuMensuelDto
{
    public int Annee { get; set; }
    public int Mois { get; set; }
    public string NomMois { get; set; } = string.Empty;
    public decimal Montant { get; set; }
}