namespace Parkini.Entities
{
    public class Acces
    {
        public int Id { get; set; }
        public DateTime DateHeureEntree { get; set; } = DateTime.UtcNow;
        public DateTime? DateHeureSortie { get; set; }
        public bool EstAutorise { get; set; } = false;
        public string? MotifRefus { get; set; }
        public string Statut { get; set; } = "EN_COURS";

        public int VehiculeId { get; set; }
        public Vehicule? Vehicule { get; set; }

        public int ParkingId { get; set; }
        public Parking? Parking { get; set; }

        public int? AbonnementId { get; set; }
        public Abonnement? Abonnement { get; set; }

        public TimeSpan? CalculerDuree()
        {
            if (DateHeureSortie.HasValue)
                return DateHeureSortie.Value - DateHeureEntree;
            return null;
        }
    }
}