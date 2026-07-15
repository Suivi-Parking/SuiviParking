<template>
    <div v-if="!authStore.isAuthenticated">
        <Login />
    </div>
    <div v-else class="flex min-h-screen bg-bg">
        <Sidebar :active="active" @navigate="active = $event" />
        <div class="flex-1 flex flex-col">
            <div class="flex justify-end items-center p-2 bg-panel border-b border-slate-200">
                <span class="text-sm text-slate-500 mr-3">{{ authStore.email }} ({{ authStore.role }})</span>
                <button @click="authStore.logout()" class="text-sm text-danger hover:underline">
                    Se déconnecter
                </button>
            </div>
            <Header :view="active" />
            <main class="flex-1">
                <EntreprisesList v-if="active === 'entreprises'" />
                <AbonnementsList v-else-if="active === 'abonnements'" />
                <ModulePlaceholder v-else :titre="titreModuleActif" />
            </main>
        </div>
    </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { authStore } from './store/auth'
import Login from './components/Login.vue'
import Sidebar from './components/Sidebar.vue'
import Header from './components/Header.vue'
import EntreprisesList from './components/EntreprisesList.vue'
import AbonnementsList from './components/AbonnementsList.vue'
import ModulePlaceholder from './components/ModulePlaceholder.vue'

const active = ref('entreprises')

const labels = {
  dashboard: 'Tableau de bord',
  utilisateurs: 'Utilisateurs',
  facturation: 'Facturation'
}
const titreModuleActif = computed(() => labels[active.value] ?? active.value)
</script>