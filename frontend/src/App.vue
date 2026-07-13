<template>
    <div class="flex min-h-screen bg-bg">
        <Sidebar :active="active" @navigate="active = $event" />
        <div class="flex-1 flex flex-col">
            <Header :view="active" />
            <main class="flex-1">
                <EntreprisesList v-if="active === 'entreprises'" />
                <AbonnementsList v-else-if="active === 'abonnements'" />
                <ParkingsList v-else-if="active === 'Parking'" />
                <ModulePlaceholder v-else :titre="titreModuleActif" />
            </main>
        </div>
    </div>
</template>

<script setup>
    import { ref, computed, watch } from 'vue'
    import Sidebar from './components/Sidebar.vue'
    import Header from './components/Header.vue'
    import EntreprisesList from './components/EntreprisesList.vue'
    import AbonnementsList from './components/AbonnementsList.vue'
    import ParkingsList from './components/ParkingsList.vue'
    import ModulePlaceholder from './components/ModulePlaceholder.vue'

    const STORAGE_KEY = 'parkini-active-view'
    const validViews = ['dashboard', 'entreprises', 'utilisateurs', 'abonnements', 'Parking', 'facturation']

    // Au premier chargement, on relit la derniere section visitee (sinon 'entreprises' par defaut)
    function getVueInitiale() {
        const saved = localStorage.getItem(STORAGE_KEY)
        return validViews.includes(saved) ? saved : 'entreprises'
    }

    const active = ref(getVueInitiale())

    // A chaque changement de section, on sauvegarde pour que F5 reste sur la meme page
    watch(active, (nouvelleValeur) => {
        localStorage.setItem(STORAGE_KEY, nouvelleValeur)
    })

    // Libelle affiche par le placeholder pour les modules pas encore construits
    const labels = {
        dashboard: 'Tableau de bord',
        utilisateurs: 'Utilisateurs',
        facturation: 'Facturation'
    }
    const titreModuleActif = computed(() => labels[active.value] ?? active.value)
</script>