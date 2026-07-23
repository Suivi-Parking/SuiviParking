namespace Parkini.Entities;

public static class RolePermissions
{
	private static readonly Dictionary<string, string[]> Mapping = new()
	{
		["Administrateur"] = new[]
		{
			Permissions.EntreprisesRead,
			Permissions.EntreprisesWrite,
			Permissions.EmployesRead,
			Permissions.EmployesWrite,
			Permissions.VehiculesRead,
			Permissions.VehiculesWrite,
		},
	};

	public static string[] GetPermissions(string? role)
	{
		if (role is null) return Array.Empty<string>();
		return Mapping.TryGetValue(role, out var permissions) ? permissions : Array.Empty<string>();
	}
}