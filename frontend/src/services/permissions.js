// Miroir des permissions définies côté backend (services/Permissions.cs).
// Garder les valeurs synchronisées avec le backend, qui reste la seule
// source de vérité pour la validation réelle (attribut [HasPermission]).
export const PERMISSIONS = {
    ENTREPRISES_READ: 'entreprises.read',
    ENTREPRISES_WRITE: 'entreprises.write',

    ABONNEMENTS_READ: 'abonnements.read',
    ABONNEMENTS_WRITE: 'abonnements.write',

    PARKINGS_READ: 'parkings.read',
    PARKINGS_WRITE: 'parkings.write',

    EMPLOYES_READ: 'employes.read',
    EMPLOYES_WRITE: 'employes.write',

    VEHICULES_READ: 'vehicules.read',
    VEHICULES_WRITE: 'vehicules.write',

    ACCES_READ: 'acces.read',
    ACCES_WRITE: 'acces.write',

    NOTIFICATIONS_READ: 'notifications.read',
    NOTIFICATIONS_WRITE: 'notifications.write',

    PAIEMENTS_READ: 'paiements.read',
    PAIEMENTS_WRITE: 'paiements.write',

    // Tableau de bord / Revenus (lecture seule : agrège Paiements + Abonnements + Entreprises)
    DASHBOARD_READ: 'dashboard.read',
}

