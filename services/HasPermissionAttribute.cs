using Microsoft.AspNetCore.Mvc.Filters;

namespace Parkini.Infrastructure.Security;

// TEMPORAIRE : le vrai systeme de permissions (verification du role/droits en base)
// n'est pas encore livre par l'equipe. Cet attribut laisse passer toutes les requetes
// pour l'instant, pour ne pas bloquer la compilation. A remplacer des que le
// systeme de permissions sera merge.
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class HasPermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permission;

    public HasPermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Ne fait rien pour l'instant : laisse passer.
        // TODO: verifier ici que l'utilisateur connecte a bien la permission `_permission`.
    }
}