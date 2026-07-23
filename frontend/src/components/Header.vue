<template>
    <header class="flex items-center justify-between p-4 bg-[var(--bg)] border-b">
        <div class="flex items-center gap-4">
            <nav class="text-sm text-muted">ParkLine › <span class="font-semibold">{{ title }}</span></nav>
        </div>
        <div class="flex items-center gap-3">
            <button v-if="showCreate" type="button" class="primary" @click="create">{{ createLabel }}</button>
        </div>
    </header>
</template>

<script setup>
    import { ref, computed } from 'vue'
    import { authStore } from '../store/auth'
    import { PERMISSIONS } from '../services/permissions'

    const props = defineProps({
        view: { type: String, default: 'entreprises' }
    })

    const q = ref('')

    const titles = {
        dashboard: 'Tableau de bord',
        entreprises: 'Entreprises clientes',
        abonnements: 'Abonnements',
        employes: 'Employés',
        vehicules: 'Véhicules',
        facturation: 'Facturation',
        notifications: 'Notifications',
    }

    const title = computed(() => titles[props.view] || props.view)

    // Permission d'écriture requise pour créer un nouvel élément dans la vue active.
    const writePermissionByView = {
        entreprises: PERMISSIONS.ENTREPRISES_WRITE,
        abonnements: PERMISSIONS.ABONNEMENTS_WRITE,
        employes: PERMISSIONS.EMPLOYES_WRITE,
        vehicules: PERMISSIONS.VEHICULES_WRITE,
        Parking: PERMISSIONS.PARKINGS_WRITE,
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
            Parking: '+ Nouveau parking'
        }
        return labels[props.view] ?? '+ Nouveau'
    })

    function create() {
        const events = {
            entreprises: 'create-entreprise',
            abonnements: 'create-abonnement',
            employes: 'create-employe',
            vehicules: 'create-vehicule',
            Parking: 'create-parking'
        }
        const eventName = events[props.view]
        if (eventName) window.dispatchEvent(new CustomEvent(eventName))
    }
</script>
