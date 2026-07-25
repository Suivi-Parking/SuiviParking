<template>
    <div class="page-container">
        <div class="page-header">
            <h1>Entreprises clientes</h1>
            <p class="subtitle">{{ totalCount }} entreprises enregistrées</p>
        </div>

        <!-- Filtres -->
        <div class="filter-bar">
            <button v-for="f in filters"
                    :key="f"
                    class="filter-btn"
                    :class="{ active: filter === f }"
                    @click="setFilter(f)">
                {{ labels[f] }}
                <span class="count-badge">{{ counts[f] ?? 0 }}</span>
            </button>
            <div class="search-wrapper">
                <input v-model="search" placeholder="Rechercher une entreprise..." class="search-input" />
            </div>
        </div>

        <!-- États -->
        <div v-if="isLoading" class="state-message">⏳ Chargement...</div>
        <div v-else-if="errorMsg" class="state-message error">
            ❌ {{ errorMsg }}
            <button class="retry-btn" @click="loadData">🔄 Réessayer</button>
        </div>

        <!-- Table -->
        <div v-else class="table-wrapper">
            <table class="data-table">
                <thead>
                    <tr>
                        <th>Nom de l'entreprise</th>
                        <th>Secteur</th>
                        <th class="text-center">Nb d'employés</th>
                        <th>Statut d'abonnement</th>
                        <th>Date de création</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    <EntrepriseRow v-for="e in paged"
                                   :key="e.id"
                                   :entreprise="e"
                                   @view="onView"
                                   @edit="openEdit"
                                   @delete="onDelete" />
                    <tr v-if="paged.length === 0">
                        <td colspan="6" class="empty-cell">📭 Aucune entreprise trouvée.</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <!-- Pagination -->
        <div class="pagination-bar">
            <button class="page-btn" :disabled="page <= 1" @click="prevPage">← Précédent</button>
            <span class="page-info">Page {{ page }} / {{ totalPages }}</span>
            <button class="page-btn" :disabled="page >= totalPages" @click="nextPage">Suivant →</button>
        </div>

        <!-- Modale formulaire -->
        <EntrepriseForm v-if="showForm"
                        :mode="formMode"
                        :entreprise="selectedEntreprise"
                        @save="onFormSave"
                        @cancel="showForm = false" />

        <!-- Toast erreur -->
        <p v-if="toastError" class="toast-error">{{ toastError }}</p>
    </div>
</template>

<script setup>
    import { ref, computed, onMounted, onUnmounted } from 'vue'
    import EntrepriseRow from './EntrepriseRow.vue'
    import EntrepriseForm from './EntrepriseForm.vue'

    // === Données ===
    const data = ref([])
    const search = ref('')
    const filter = ref('Tous')
    const page = ref(1)
    const pageSize = ref(10)
    const isLoading = ref(false)
    const errorMsg = ref('')
    const toastError = ref('')

    const filters = ['Tous', 'Actif', 'EnAttente', 'Expire']
    const labels = { Tous: 'Tous', Actif: 'Actif', EnAttente: 'En attente', Expire: 'Expiré' }
    const counts = ref({ Tous: 0, Actif: 0, EnAttente: 0, Expire: 0 })
    const totalCount = ref(0)

    // === Modal form ===
    const showForm = ref(false)
    const formMode = ref('create')
    const selectedEntreprise = ref(null)

    // === Données fictives (si API indisponible) ===
    const MOCK_DATA = [
        {
            id: 1,
            nom: 'Groupe Renault Industries',
            matriculeFiscal: 'GRI-001',
            secteur: 'Automobile',
            nombreEmployes: 4820,
            statutAbonnement: 'Actif',
            dateCreation: '2023-01-12T00:00:00.000Z',
        },
        {
            id: 2,
            nom: 'BNP Paribas Asset Management',
            matriculeFiscal: 'BNP-002',
            secteur: 'Finance',
            nombreEmployes: 1340,
            statutAbonnement: 'Actif',
            dateCreation: '2023-03-03T00:00:00.000Z',
        },
        {
            id: 3,
            nom: 'Veolia Environnement',
            matriculeFiscal: 'VE-003',
            secteur: 'Énergie',
            nombreEmployes: 890,
            statutAbonnement: 'EnAttente',
            dateCreation: '2023-04-17T00:00:00.000Z',
        },
        {
            id: 4,
            nom: 'Samsung Electronics Tunisie',
            matriculeFiscal: 'SAM-004',
            secteur: 'Électronique',
            nombreEmployes: 240,
            statutAbonnement: 'Expire',
            dateCreation: '2022-11-05T00:00:00.000Z',
        },
    ]

    // === Chargement des données ===
    async function loadData() {
        isLoading.value = true
        errorMsg.value = ''
        try {
            const res = await fetch('/api/entreprises')
            if (!res.ok) throw new Error('Erreur chargement')
            const json = await res.json()
            const items = json.items || json || []
            data.value = items
            computeCounts()
        } catch (err) {
            console.warn('⚠️ Backend injoignable, utilisation des données fictives')
            data.value = MOCK_DATA
            errorMsg.value = 'Impossible de contacter le serveur. Affichage des données de démonstration.'
            computeCounts()
        } finally {
            isLoading.value = false
        }
    }

    function computeCounts() {
        counts.value.Tous = data.value.length
        counts.value.Actif = data.value.filter(x => x.statutAbonnement === 'Actif').length
        counts.value.EnAttente = data.value.filter(x => x.statutAbonnement === 'EnAttente').length
        counts.value.Expire = data.value.filter(x => x.statutAbonnement === 'Expire').length
        totalCount.value = data.value.length
    }

    // === Filtres ===
    function setFilter(f) {
        filter.value = f
        page.value = 1
    }

    // === Navigation ===
    function openCreate() {
        formMode.value = 'create'
        selectedEntreprise.value = null
        showForm.value = true
    }

    function openEdit(entreprise) {
        formMode.value = 'edit'
        selectedEntreprise.value = entreprise
        showForm.value = true
    }

    function onView(entreprise) {
        // Pour l'instant, on peut afficher une alerte ou ouvrir un modal de détails
        alert(JSON.stringify(entreprise, null, 2))
    }

    // === Sauvegarde (création/modification) ===
    async function onFormSave(payload) {
        toastError.value = ''
        try {
            const url = formMode.value === 'create' ? '/api/entreprises' : `/api/entreprises/${selectedEntreprise.value.id}`
            const method = formMode.value === 'create' ? 'POST' : 'PUT'
            const res = await fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload),
            })
            if (!res.ok) {
                const msg = await res.text()
                toastError.value = msg || `Erreur ${res.status}`
                return
            }
            showForm.value = false
            await loadData()
        } catch {
            toastError.value = 'Impossible de contacter le serveur.'
        }
    }

    // === Suppression ===
    async function onDelete(entreprise) {
        if (!confirm(`Supprimer l'entreprise "${entreprise.nom}" ?`)) return
        try {
            const res = await fetch(`/api/entreprises/${entreprise.id}`, { method: 'DELETE' })
            if (!res.ok) {
                const msg = await res.text()
                toastError.value = msg || `Erreur ${res.status}`
                return
            }
            await loadData()
        } catch {
            toastError.value = 'Impossible de contacter le serveur.'
        }
    }

    // === Pagination & filtrage ===
    const filtered = computed(() => {
        let list = data.value || []
        if (search.value) {
            const q = search.value.toLowerCase()
            list = list.filter(
                (x) =>
                    (x.nom || '').toLowerCase().includes(q) ||
                    (x.matriculeFiscal || '').toLowerCase().includes(q)
            )
        }
        if (filter.value !== 'Tous') {
            list = list.filter((x) => x.statutAbonnement === filter.value)
        }
        return list
    })

    const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / pageSize.value)))
    const paged = computed(() => {
        const start = (page.value - 1) * pageSize.value
        return filtered.value.slice(start, start + pageSize.value)
    })

    function prevPage() {
        if (page.value > 1) page.value--
    }
    function nextPage() {
        if (page.value < totalPages.value) page.value++
    }

    // === Lifecycle ===
    onMounted(() => {
        loadData()
        window.addEventListener('create-entreprise', openCreate)
    })
    onUnmounted(() => {
        window.removeEventListener('create-entreprise', openCreate)
    })
</script>

<style scoped>
    /* === STYLES IDENTIQUES AUX ABONNEMENTS === */
    .page-container {
        padding: 1.5rem 2rem;
    }

    .page-header {
        margin-bottom: 1.5rem;
    }

        .page-header h1 {
            font-size: 1.6rem;
            font-weight: 700;
            color: #0b1a2f;
            margin: 0 0 0.2rem;
        }

    .subtitle {
        color: #64748b;
        font-size: 0.9rem;
    }

    .filter-bar {
        display: flex;
        flex-wrap: wrap;
        align-items: center;
        gap: 0.5rem;
        margin-bottom: 1.5rem;
    }

    .filter-btn {
        padding: 0.4rem 1rem;
        border-radius: 30px;
        border: 1px solid #e2e8f0;
        background: #fff;
        font-size: 0.85rem;
        font-weight: 500;
        color: #1e293b;
        cursor: pointer;
        transition: all 0.2s;
        display: inline-flex;
        align-items: center;
        gap: 0.4rem;
    }

        .filter-btn:hover {
            background: #f1f5f9;
        }

        .filter-btn.active {
            background: #106ad7;
            color: #fff;
            border-color: #106ad7;
            box-shadow: 0 2px 8px rgba(16, 106, 215, 0.25);
        }

    .count-badge {
        background: rgba(0, 0, 0, 0.06);
        padding: 0 0.5rem;
        border-radius: 20px;
        font-size: 0.7rem;
        font-weight: 600;
    }

    .filter-btn.active .count-badge {
        background: rgba(255, 255, 255, 0.2);
        color: #fff;
    }

    .search-wrapper {
        margin-left: auto;
    }

    .search-input {
        padding: 0.4rem 1rem;
        border: 1px solid #e2e8f0;
        border-radius: 30px;
        font-size: 0.85rem;
        min-width: 200px;
        transition: border 0.2s;
    }

        .search-input:focus {
            border-color: #106ad7;
            outline: none;
            box-shadow: 0 0 0 3px rgba(16, 106, 215, 0.12);
        }

    .state-message {
        padding: 2rem;
        text-align: center;
        color: #64748b;
        font-size: 1rem;
    }

        .state-message.error {
            color: #dc2626;
        }

    .retry-btn {
        margin-left: 1rem;
        padding: 0.3rem 1rem;
        border: 1px solid #dc2626;
        border-radius: 30px;
        background: #fff;
        color: #dc2626;
        cursor: pointer;
    }

        .retry-btn:hover {
            background: #fee2e2;
        }

    .table-wrapper {
        background: #fff;
        border-radius: 16px;
        box-shadow: 0 4px 16px rgba(0, 0, 0, 0.04);
        overflow-x: auto;
        border: 1px solid #f1f5f9;
    }

    .data-table {
        width: 100%;
        border-collapse: collapse;
        font-size: 0.9rem;
    }

        .data-table thead {
            background: #f8fafc;
            border-bottom: 2px solid #eef2f6;
        }

        .data-table th {
            padding: 0.8rem 0.8rem;
            text-align: left;
            font-weight: 600;
            font-size: 0.75rem;
            text-transform: uppercase;
            letter-spacing: 0.03em;
            color: #475569;
        }

            .data-table th.text-center {
                text-align: center;
            }

        .data-table tbody tr:last-child td {
            border-bottom: none;
        }

    .empty-cell {
        text-align: center;
        padding: 2rem;
        color: #94a3b8;
    }

    .pagination-bar {
        display: flex;
        justify-content: center;
        align-items: center;
        gap: 1rem;
        margin-top: 1.5rem;
    }

    .page-btn {
        padding: 0.4rem 1.2rem;
        border-radius: 30px;
        border: 1px solid #e2e8f0;
        background: #fff;
        font-size: 0.85rem;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.2s;
    }

        .page-btn:hover:not(:disabled) {
            background: #f1f5f9;
        }

        .page-btn:disabled {
            opacity: 0.4;
            cursor: not-allowed;
        }

    .page-info {
        font-size: 0.9rem;
        color: #475569;
    }

    .toast-error {
        position: fixed;
        bottom: 1.5rem;
        right: 1.5rem;
        background: #dc2626;
        color: #fff;
        padding: 0.6rem 1.4rem;
        border-radius: 12px;
        box-shadow: 0 8px 24px rgba(220, 38, 38, 0.3);
        font-size: 0.9rem;
        z-index: 70;
    }
</style>