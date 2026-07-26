namespace Parkini.Infrastructure.Security;

/// - Admin   : toutes les permissions (accès total).
/// - Manager : permissions limitées, lecture seule (toutes les permissions ".read").

public static class RolePermissions
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";

    private static readonly IReadOnlyDictionary<string, HashSet<string>> Map =
        new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
        {
            [Admin] = new HashSet<string>(Permissions.All, StringComparer.OrdinalIgnoreCase),

            [Manager] = new HashSet<string>(
                Permissions.All.Where(p => p.EndsWith(".read", StringComparison.OrdinalIgnoreCase)),
                StringComparer.OrdinalIgnoreCase),
        };

    ///Retourne l'ensemble des permissions accordées à un rôle donné.
    public static IReadOnlySet<string> GetPermissions(string? roleName)
    {
        if (!string.IsNullOrWhiteSpace(roleName) && Map.TryGetValue(roleName, out var permissions))
            return permissions;

        return new HashSet<string>();
    }

    ///Indique si un rôle donné dispose d'une permission précise.
    public static bool HasPermission(string? roleName, string permission) =>
        GetPermissions(roleName).Contains(permission);
}
