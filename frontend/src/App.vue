<template>
    <div class="flex min-h-screen bg-bg">
        <Sidebar :active="active" @navigate="active = $event" />
        <div class="flex-1 flex flex-col">
            <Header :view="active" />
            <main class="flex-1">
                <EntreprisesList v-if="active === 'entreprises'" />
                <AbonnementsList v-else-if="active === 'abonnements'" />
                <EmployesList v-else-if="active === 'employes'" />
                <VehiculesList v-else-if="active === 'vehicules'" />
                <ModulePlaceholder v-else :titre="titreModuleActif" />
            </main>
        </div>
    </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import Sidebar from './components/Sidebar.vue'
import Header from './components/Header.vue'
import EntreprisesList from './components/EntreprisesList.vue'
import AbonnementsList from './components/AbonnementsList.vue'
import EmployesList from './components/EmployesList.vue'
import VehiculesList from './components/VehiculesList.vue'
import ModulePlaceholder from './components/ModulePlaceholder.vue'

const active = ref('entreprises')

const labels = {
    dashboard: 'Tableau de bord',
    utilisateurs: 'Utilisateurs',
    facturation: 'Facturation'
}
const titreModuleActif = computed(() => labels[active.value] ?? active.value)
</script>