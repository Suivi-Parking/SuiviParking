using SuiviParking.API.Models;

namespace Parkini.Entities
{
    public class Vehicule
    {
        public int Id { get; set; }
        public string Immatriculation { get; set; } = string.Empty;
        public string Marque { get; set; } = string.Empty;
        public string Modele { get; set; } = string.Empty;
        public string Couleur { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Statut { get; set; } = "ACTIF";

        // Relation
        public int EmployeId { get; set; }
        public Employe? Employe { get; set; }
    }
}
