<template>
    <header class="flex items-center justify-between p-4 bg-panel border-b">
        <div class="flex items-center gap-4">
            <nav class="text-sm text-gray-400">ParkLine › <span class="font-semibold text-gray-700">{{ title }}</span></nav>
        </div>
        <div class="flex items-center gap-3">
            <input v-model="q" placeholder="Rechercher..." class="px-4 py-2 rounded-lg bg-white border" />
            <button v-if="showCreate" type="button" class="bg-primary text-white px-4 py-2 rounded-lg" @click="create">{{ createLabel }}</button>
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
        utilisateurs: 'Utilisateurs',
        abonnements: 'Abonnements',
        Parking: 'Gestion des parkings',
        facturation: 'Facturation',
    }

    const title = computed(() => titles[props.view] || props.view)

    // Vues ayant une action de creation depuis le Header
    const createConfig = {
        entreprises: { label: '+ Nouvelle entreprise', event: 'create-entreprise' },
        abonnements: { label: '+ Nouvel abonnement', event: 'create-abonnement' },
        Parking: { label: '+ Nouveau parking', event: 'create-parking' },
    }

    const showCreate = computed(() => !!createConfig[props.view])
    const createLabel = computed(() => createConfig[props.view]?.label ?? '')

    function create() {
        const config = createConfig[props.view]
        if (!config) return
        window.dispatchEvent(new CustomEvent(config.event))
    }
</script>