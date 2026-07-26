<template>
    <header class="app-header">
        <div class="header-left">
            <nav class="breadcrumb">ParkLine › <span class="current">{{ title }}</span></nav>
        </div>
        <div class="header-right">
            <button v-if="showCreate" type="button" class="btn-create" @click="create">
                {{ createLabel }}
            </button>
        </div>
    </header>
</template>

<script setup>
    import { computed } from 'vue'
    import { authStore } from '../store/auth'
    import { PERMISSIONS } from '../services/permissions'

    const props = defineProps({
        view: { type: String, default: 'entreprises' }
    })

    const titles = {
        dashboard: 'Tableau de bord',
        entreprises: 'Entreprises clientes',
        abonnements: 'Abonnements',
        employes: 'Employés',
        vehicules: 'Véhicules',
        paiements: 'Paiements & échéances',
        notifications: 'Notifications',
        Parking: 'Parkings',
        acces: 'Accès',
    }

    const title = computed(() => titles[props.view] || props.view)

    const writePermissionByView = {
        entreprises: PERMISSIONS.ENTREPRISES_WRITE,
        abonnements: PERMISSIONS.ABONNEMENTS_WRITE,
        employes: PERMISSIONS.EMPLOYES_WRITE,
        vehicules: PERMISSIONS.VEHICULES_WRITE,
        Parking: PERMISSIONS.PARKINGS_WRITE,
        paiements: PERMISSIONS.PAIEMENTS_WRITE,
    }

    const showCreate = computed(() => {
        const requiredPermission = writePermissionByView[props.view]
        return !!requiredPermission && authStore.can(requiredPermission)
    })

    const createLabel = computed(() => {
        const labels = {
            entreprises: '+ Nouvelle entreprise',
            abonnements: '+ Nouvel abonnement',
            employes: '+ Nouvel employé',
            vehicules: '+ Nouveau véhicule',
            Parking: '+ Nouveau parking',
            paiements: '+ Nouveau paiement',
        }
        return labels[props.view] ?? '+ Nouveau'
    })

    function create() {
        const events = {
            entreprises: 'create-entreprise',
            abonnements: 'create-abonnement',
            employes: 'create-employe',
            vehicules: 'create-vehicule',
            Parking: 'create-parking',
            paiements: 'create-paiement',
        }
        const eventName = events[props.view]
        if (eventName) window.dispatchEvent(new CustomEvent(eventName))
    }
</script>

<style scoped>
    .app-header {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 1rem 2rem;
        background: rgba(255, 255, 255, 0.6);
        backdrop-filter: blur(8px);
        border-bottom: 1px solid rgba(0, 0, 0, 0.04);
        box-shadow: 0 2px 12px rgba(0, 0, 0, 0.02);
    }
    .breadcrumb {
        font-size: 0.95rem;
        color: #6b7280;
    }
    .breadcrumb .current {
        font-weight: 600;
        color: #1e293b;
    }
    .btn-create {
        padding: 0.5rem 1.2rem;
        background: linear-gradient(135deg, #106ad7, #0b57c3);
        border: none;
        border-radius: 30px;
        font-weight: 500;
        font-size: 0.9rem;
        color: #fff;
        cursor: pointer;
        box-shadow: 0 4px 12px rgba(16, 106, 215, 0.3);
        transition: all 0.2s;
    }
    .btn-create:hover {
        transform: translateY(-2px);
        box-shadow: 0 8px 20px rgba(16, 106, 215, 0.4);
    }
</style>