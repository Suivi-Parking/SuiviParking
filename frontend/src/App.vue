<template>
    <div v-if="!authStore.isAuthenticated">
        <Login />
    </div>
    <div v-else class="flex min-h-screen bg-bg">
        <Sidebar :active="active" @navigate="active = $event" />
        <div class="flex-1 flex flex-col">
            <div class="w-full flex justify-end items-center gap-4 px-4 py-2 bg-panel border-b border-slate-200">
                <span class="text-sm text-slate-500 whitespace-nowrap truncate max-w-[280px]">
                    {{ authStore.email }} ({{ authStore.role }})
                </span>
                <button
                    @click="authStore.logout()"
                    class="text-sm text-danger hover:underline whitespace-nowrap flex-shrink-0"
                >
                    Se déconnecter
                </button>
            </div>
            <Header :view="active" />
            <main class="flex-1">
                <EntreprisesList v-if="active === 'entreprises'" />
                <AbonnementsList v-else-if="active === 'abonnements'" />
                <ParkingsList v-else-if="active === 'Parking'" />
                <EmployesList v-else-if="active === 'employes'" />
                <VehiculesList v-else-if="active === 'vehicules'" />
                <PaiementsList v-else-if="active === 'paiements'" />
                <AccesList v-else-if="active === 'acces'" />
                <NotificationsList v-else-if="active === 'notifications'" />
                <DashboardHome v-else-if="active === 'dashboard'" />
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
    import EntreprisesList from './components/EntreprisesList.vue'
    import AbonnementsList from './components/AbonnementsList.vue'
    import PaiementsList from './components/PaiementsList.vue'
    import ParkingsList from './components/ParkingsList.vue'
    import EmployesList from './components/EmployesList.vue'
    import VehiculesList from './components/VehiculesList.vue'
    import AccesList from './components/AccesList.vue'
    import NotificationsList from './components/NotificationsList.vue'
    import DashboardHome from './components/DashboardHome.vue'
    import ModulePlaceholder from './components/ModulePlaceholder.vue'

    const active = ref('dashboard')

    const labels = {
        utilisateurs: 'Utilisateurs',
        facturation: 'Facturation'
    }
    const titreModuleActif = computed(() => labels[active.value] ?? active.value)
</script>