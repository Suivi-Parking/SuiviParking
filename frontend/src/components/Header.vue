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
}

const title = computed(() => titles[props.view] || props.view)

const showCreate = computed(() =>
    ['entreprises', 'abonnements', 'employes', 'vehicules', 'Parking'].includes(props.view)
)

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