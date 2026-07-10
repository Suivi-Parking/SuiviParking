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
        utilisateurs: 'Utilisateurs',
        abonnements: 'Abonnements',
        facturation: 'Facturation',
    }

    const title = computed(() => titles[props.view] || props.view)

    // Seules les vues Entreprises et Abonnements ont une action de creation pour l'instant
    const showCreate = computed(() => props.view === 'entreprises' || props.view === 'abonnements')

    const createLabel = computed(() =>
        props.view === 'abonnements' ? '+ Nouvel abonnement' : '+ Nouvelle entreprise'
    )

    function create() {
        const eventName = props.view === 'abonnements' ? 'create-abonnement' : 'create-entreprise'
        window.dispatchEvent(new CustomEvent(eventName))
    }
</script>