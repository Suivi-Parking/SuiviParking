namespace Parkini.Application.DTOs;

// ===== Bloc "Parkings" =====

public class ParkingOccupationDto
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public int CapaciteMax { get; set; }
    public int PlacesOccupees { get; set; }
    public decimal TauxOccupation { get; set; } // en %
}

public class AlerteSaturationDto
{
    public int ParkingId { get; set; }
    public string ParkingNom { get; set; } = string.Empty;
    public decimal TauxOccupation { get; set; }
    public int PlacesLibres { get; set; }
}

public class PointEvolutionOccupationDto
{
    // Libellé de l'axe X : heure ("08:00") pour la vue "jour", jour ("Lun 27/07") pour la vue "semaine"
    public string Label { get; set; } = string.Empty;
    public DateTime Instant { get; set; }
    public int VehiculesPresents { get; set; }
    public decimal TauxOccupation { get; set; } // en %, sur la capacité totale connue à cet instant
}

public class DashboardParkingsDto
{
    public int NombreTotalParkings { get; set; }
    public decimal TauxOccupationGlobal { get; set; }
    public ParkingOccupationDto? ParkingLePlusOccupe { get; set; }
    public ParkingOccupationDto? ParkingLeMoinsOccupe { get; set; }
    public List<AlerteSaturationDto> AlertesSaturation { get; set; } = new();
}

// ===== Bloc "Financier" =====

public class RepartitionAbonnementTypeDto
{
    public string Type { get; set; } = string.Empty;
    public int Nombre { get; set; }
}

public class RevenuEntrepriseDto
{
    public int EntrepriseId { get; set; }
    public string EntrepriseNom { get; set; } = string.Empty;
    public decimal MontantTotal { get; set; }
    public int NombrePaiements { get; set; }
    public decimal PourcentageDuTotal { get; set; } // en %, calcule sur le total filtre
}

public class PaiementsParMoisDto
{
    public string Mois { get; set; } = string.Empty; // ex: "Juil. 2026"
    public int Annee { get; set; }
    public int MoisNumero { get; set; }
    public decimal MontantPercu { get; set; }
}

public class DashboardFinancierDto
{
    public decimal MontantPercuMoisEnCours { get; set; }
    public decimal MontantPercuCumule { get; set; }
    public decimal MontantImpayeTotal { get; set; }
    public int NombrePaiementsEnRetard { get; set; }
    public int NombreAbonnementsExpirantSous30Jours { get; set; }
    public List<RepartitionAbonnementTypeDto> RepartitionAbonnements { get; set; } = new();
    public List<RevenuEntrepriseDto> RevenusParEntreprise { get; set; } = new();
    public decimal RevenuTotalToutesEntreprises { get; set; }
}

// ===== Rapport dédié : "En tant qu'Administrateur, je veux consulter les
// revenus totaux et par entreprise afin de piloter la performance financière" =====

public class RevenusRapportDto
{
    public decimal RevenuTotal { get; set; }
    public int NombrePaiements { get; set; }
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public List<RevenuEntrepriseDto> ParEntreprise { get; set; } = new();
}

// ===== Widget : dernières notifications =====

public class DashboardSummaryDto
{
    public DashboardParkingsDto Parkings { get; set; } = new();
    public DashboardFinancierDto Financier { get; set; } = new();
    public List<NotificationDto> DernieresNotifications { get; set; } = new();
    public DateTime GenereLe { get; set; } = DateTime.UtcNow;
}
