using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Parkini.Infrastructure.Security;

/// Attribut à poser sur un contrôleur ou une action pour exiger une permission précise.
/// Valide, à chaque appel, que l'utilisateur authentifié (via son rôle porté par le JWT)
/// dispose bien de la permission demandée. Sinon, renvoie 401 (non authentifié)
/// ou 403 (authentifié mais permission manquante).

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class HasPermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permission;

    public HasPermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user?.Identity is null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Authentification requise."
            });
            return;
        }

        var role = user.FindFirst(ClaimTypes.Role)?.Value;

        if (!RolePermissions.HasPermission(role, _permission))
        {
            context.Result = new ObjectResult(new
            {
                message = $"Accès refusé : la permission '{_permission}' est requise pour cette action.",
                permission = _permission,
                role
            })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
