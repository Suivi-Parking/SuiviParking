<template>
    <div v-if="!authStore.isAuthenticated">
        <Login />
    </div>
    <div v-else class="flex min-h-screen bg-bg">
        <Sidebar :active="active" @navigate="active = $event" />
        <div class="flex-1 flex flex-col">
            
            <Header :view="active" />
            <main class="flex-1">
                <Dashboard v-if="active === 'dashboard'" @navigate="active = $event" />
                <EntreprisesList v-else-if="active === 'entreprises'" />
                <AbonnementsList v-else-if="active === 'abonnements'" />
                <ParkingsList v-else-if="active === 'Parking'" />
                <EmployesList v-else-if="active === 'employes'" />
                <VehiculesList v-else-if="active === 'vehicules'" />
                <PaiementsList v-else-if="active === 'paiements'" />
                <Revenus v-else-if="active === 'revenus'" />
                <AccesList v-else-if="active === 'acces'" />
                <NotificationsList v-else-if="active === 'notifications'" />
                <ModulePlaceholder v-else :titre="titreModuleActif" />
            </main>
        </div>
    </div>
</template>
<script setup>
    import { ref, computed } from 'vue'
    import { authStore } from './store/auth'
    import Sidebar from './components/Sidebar.vue'
    import Header from './components/Header.vue'
    import Login from './components/Login.vue'
    import Dashboard from './components/Dashboard.vue'
    import EntreprisesList from './components/EntreprisesList.vue'
    import AbonnementsList from './components/AbonnementsList.vue'
    import PaiementsList from './components/PaiementsList.vue'
    import Revenus from './components/Revenus.vue'
    import ParkingsList from './components/ParkingsList.vue'
    import EmployesList from './components/EmployesList.vue'
    import VehiculesList from './components/VehiculesList.vue'
    import AccesList from './components/AccesList.vue'
    import NotificationsList from './components/NotificationsList.vue'
    import ModulePlaceholder from './components/ModulePlaceholder.vue'

    const active = ref('dashboard')

    const labels = {
        dashboard: 'Tableau de bord',
        utilisateurs: 'Utilisateurs',
        facturation: 'Facturation'
    }
    const titreModuleActif = computed(() => labels[active.value] ?? active.value)
</script>