namespace Parkini.Infrastructure.Security;

/// Catalogue centralisé de toutes les permissions applicatives.
public static class Permissions
{
    // Entreprises
    public const string EntreprisesRead = "entreprises.read";
    public const string EntreprisesWrite = "entreprises.write";

    // Abonnements
    public const string AbonnementsRead = "abonnements.read";
    public const string AbonnementsWrite = "abonnements.write";

    // Parkings
    public const string ParkingsRead = "parkings.read";
    public const string ParkingsWrite = "parkings.write";

    // Employés
    public const string EmployesRead = "employes.read";
    public const string EmployesWrite = "employes.write";

    // Véhicules
    public const string VehiculesRead = "vehicules.read";
    public const string VehiculesWrite = "vehicules.write";

    // Utilisateurs / rôles
    public const string UtilisateursRead = "utilisateurs.read";
    public const string UtilisateursWrite = "utilisateurs.write";

    /// <summary>Toutes les permissions existantes dans le système.</summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        EntreprisesRead, EntreprisesWrite,
        AbonnementsRead, AbonnementsWrite,
        ParkingsRead, ParkingsWrite,
        EmployesRead, EmployesWrite,
        VehiculesRead, VehiculesWrite,
        UtilisateursRead, UtilisateursWrite,
    };
}
