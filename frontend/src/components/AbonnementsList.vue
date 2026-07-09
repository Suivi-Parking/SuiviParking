<script setup>
    import { ref, reactive, computed, onMounted, onUnmounted } from 'vue'
    import AbonnementRow from './AbonnementRow.vue'
    import AbonnementForm from './AbonnementForm.vue'
    import AbonnementActionModal from './AbonnementActionModal.vue'

    const data = ref([])
    const entreprises = ref([]) // pour le select du formulaire de creation
    const filter = ref('Tous')
    const page = ref(1)
    const pageSize = ref(10)
    const isLoading = ref(false)
    const errorMsg = ref('')

    const filters = ['Tous', 'Actif', 'Suspendu', 'Resilie', 'Expire']
    const labels = {
        Tous: 'Tous',
        Actif: 'Actif',
        Suspendu: 'Suspendu',
        Resilie: 'Résilié',
        Expire: 'Expiré'
    }
    const counts = reactive({ Tous: 0, Actif: 0, Suspendu: 0, Resilie: 0, Expire: 0 })
    const totalCount = ref(0)

    const showForm = ref(false)
    const actionModal = reactive({ visible: false, mode: null, abonnement: null })

    async function loadEntreprises() {
        try {
            const res = await fetch('/api/entreprises?pageSize=100')
            if (!res.ok) return
            const json = await res.json()
            entreprises.value = json.items ?? []
        } catch {
            entreprises.value = []
        }
    }

    async function loadData() {
        isLoading.value = true
        errorMsg.value = ''
        try {
            const params = new URLSearchParams({ page: page.value, pageSize: pageSize.value })
            if (filter.value !== 'Tous') params.set('statut', filter.value)

            const res = await fetch(`/api/abonnements?${params.toString()}`)
            if (!res.ok) throw new Error('Requete echouee')
            const json = await res.json()

            data.value = json.items ?? []
            totalCount.value = json.totalCount ?? 0
            counts.Tous = json.counts?.tous ?? 0
            counts.Actif = json.counts?.actif ?? 0
            counts.Suspendu = json.counts?.suspendu ?? 0
            counts.Resilie = json.counts?.resilie ?? 0
            counts.Expire = json.counts?.expire ?? 0
        } catch (e) {
            errorMsg.value = "Impossible de charger les abonnements. Verifiez que le backend tourne."
            data.value = []
        } finally {
            isLoading.value = false
        }
    }

    function setFilter(f) {
        filter.value = f
        page.value = 1
        loadData()
    }

    function openCreate() {
        showForm.value = true
    }

    const createError = ref('')

    // Le formulaire emet 'save' avec les donnees brutes : c'est ici qu'on
    // fait reellement l'appel POST vers le backend.
    async function onSave(formData) {
        createError.value = ''
        try {
            const res = await fetch('/api/abonnements', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(formData)
            })

            if (!res.ok) {
                const message = await res.text()
                // 409 = regle metier "deja un abonnement actif", 400 = validation
                createError.value = message || `Erreur ${res.status}`
                return
            }

            showForm.value = false
            await loadData()
        } catch (e) {
            createError.value = "Impossible de contacter le serveur."
        }
    }

    function openAction(mode, abonnement) {
        actionModal.mode = mode
        actionModal.abonnement = abonnement
        actionModal.visible = true
    }

    async function onActionConfirmed() {
        actionModal.visible = false
        await loadData()
    }

    const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize.value)))

    onMounted(() => {
        loadEntreprises()
        loadData()
        window.addEventListener('create-abonnement', openCreate)
    })

    onUnmounted(() => {
        window.removeEventListener('create-abonnement', openCreate)
    })
</script>

<template>
    <div class="p-6">
        <div class="flex items-center justify-between mb-4">
            <p class="text-sm text-gray-500">{{ totalCount }} abonnement(s) enregistré(s)</p>
        </div>

        <div class="flex gap-2 mb-4">
            <button v-for="f in filters"
                    :key="f"
                    class="px-4 py-2 rounded-full border text-sm"
                    :class="filter === f ? 'bg-primary text-white border-primary' : 'bg-white border-gray-200'"
                    @click="setFilter(f)">
                {{ labels[f] }} <span class="ml-1 opacity-70">{{ counts[f] ?? 0 }}</span>
            </button>
        </div>

        <p v-if="isLoading" class="text-gray-500">Chargement...</p>
        <p v-else-if="errorMsg" class="text-danger">{{ errorMsg }}</p>

        <table v-else class="w-full bg-panel rounded-xl overflow-hidden">
            <thead>
                <tr class="text-left text-xs uppercase text-gray-500 border-b">
                    <th class="p-3">Entreprise</th>
                    <th class="p-3">Type</th>
                    <th class="p-3">Début</th>
                    <th class="p-3">Fin</th>
                    <th class="p-3">Montant</th>
                    <th class="p-3">Statut</th>
                    <th class="p-3">Actions</th>
                </tr>
            </thead>
            <tbody>
                <AbonnementRow v-for="a in data"
                               :key="a.id"
                               :abonnement="a"
                               @renouveler="openAction('renouveler', a)"
                               @suspendre="openAction('suspendre', a)"
                               @resilier="openAction('resilier', a)" />
                <tr v-if="data.length === 0">
                    <td colspan="7" class="text-center p-6 text-gray-400">Aucun abonnement trouvé.</td>
                </tr>
            </tbody>
        </table>

        <div class="flex items-center justify-center gap-4 mt-4">
            <button :disabled="page <= 1" @click="page--; loadData()">←</button>
            <span class="text-sm">Page {{ page }} / {{ totalPages }}</span>
            <button :disabled="page >= totalPages" @click="page++; loadData()">→</button>
        </div>

        <div v-if="showForm">
            <AbonnementForm :entreprises="entreprises"
                            @save="onSave"
                            @cancel="showForm = false" />
            <p v-if="createError" class="fixed bottom-4 right-4 bg-danger text-white px-4 py-2 rounded-lg shadow-lg text-sm">
                {{ createError }}
            </p>
        </div>

        <AbonnementActionModal v-if="actionModal.visible"
                               :mode="actionModal.mode"
                               :abonnement="actionModal.abonnement"
                               @confirmed="onActionConfirmed"
                               @close="actionModal.visible = false" />
    </div>
</template>