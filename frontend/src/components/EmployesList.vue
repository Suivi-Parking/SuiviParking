<template>
    <div class="page-container">
        <div class="page-header">
            <h1>Employés</h1>
            <p class="subtitle">{{ totalCount }} employés enregistrés</p>
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
                <input v-model="search" placeholder="Rechercher un employé..." class="search-input" />
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
                        <th>Employé</th>
                        <th>Email</th>
                        <th>Téléphone</th>
                        <th>Fonction</th>
                        <th>Statut</th>
                        <th>Véhicules</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    <EmployeRow v-for="e in paged"
                                :key="e.id"
                                :employe="e"
                                @view="onView"
                                @edit="openEdit"
                                @delete="confirmDelete" />
                    <tr v-if="paged.length === 0">
                        <td colspan="7" class="empty-cell">📭 Aucun employé trouvé.</td>
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

        <!-- Modales -->
        <EmployeForm v-if="showForm"
                     :mode="formMode"
                     :employe="selectedEmploye"
                     @save="onFormSave"
                     @cancel="showForm = false" />

        <ModalConfirm v-if="showConfirmDelete"
                      title="Supprimer l'employé"
                      :message="`Êtes-vous sûr de vouloir supprimer ${employeToDelete?.nom} ${employeToDelete?.prenom} ?`"
                      confirmLabel="Supprimer"
                      cancelLabel="Annuler"
                      type="danger"
                      @confirm="onDelete"
                      @cancel="showConfirmDelete = false" />

        <ModalConfirm v-if="showError"
                      title="Erreur"
                      :message="errorMessage"
                      confirmLabel="OK"
                      :showCancel="false"
                      type="warning"
                      @confirm="showError = false" />

        <ModalDetail v-if="showDetail"
                     title="Détails de l'employé"
                     :fields="detailFields"
                     @close="showDetail = false" />

        <p v-if="toastError" class="toast-error">{{ toastError }}</p>
    </div>
</template>

<script setup>
    import { ref, computed, onMounted, onUnmounted } from 'vue'
    import EmployeRow from './EmployeRow.vue'
    import EmployeForm from './EmployeForm.vue'
    import ModalConfirm from './ModalConfirm.vue'
    import ModalDetail from './ModalDetail.vue'

    // === Données ===
    const data = ref([])
    const search = ref('')
    const filter = ref('Tous')
    const page = ref(1)
    const pageSize = ref(10)
    const isLoading = ref(false)
    const errorMsg = ref('')
    const toastError = ref('')

    const filters = ['Tous', 'ACTIF', 'INACTIF']
    const labels = { Tous: 'Tous', ACTIF: 'Actif', INACTIF: 'Inactif' }
    const counts = ref({ Tous: 0, ACTIF: 0, INACTIF: 0 })
    const totalCount = ref(0)

    // === Modals ===
    const showForm = ref(false)
    const formMode = ref('create')
    const selectedEmploye = ref(null)

    const showConfirmDelete = ref(false)
    const employeToDelete = ref(null)
    const showError = ref(false)
    const errorMessage = ref('')
    const showDetail = ref(false)
    const detailFields = ref([])

    // === Données fictives (si API indisponible) ===
    const MOCK_DATA = [
        {
            id: 1,
            nom: 'Dupont',
            prenom: 'Jean',
            email: 'jean.dupont@exemple.com',
            telephone: '71234567',
            fonction: 'Développeur',
            entrepriseId: 1,
            statut: 'ACTIF',
            vehicules: [{ immatriculation: 'TUN-123' }],
            dateRattachement: '2025-01-15T00:00:00.000Z'
        },
        {
            id: 2,
            nom: 'Martin',
            prenom: 'Sophie',
            email: 'sophie.martin@exemple.com',
            telephone: '72345678',
            fonction: 'Manager',
            entrepriseId: 2,
            statut: 'INACTIF',
            vehicules: [],
            dateRattachement: '2024-11-03T00:00:00.000Z'
        },
        {
            id: 3,
            nom: 'Bernard',
            prenom: 'Pierre',
            email: 'pierre.bernard@exemple.com',
            telephone: '73456789',
            fonction: 'Commercial',
            entrepriseId: 1,
            statut: 'ACTIF',
            vehicules: [{ immatriculation: 'TUN-456' }],
            dateRattachement: '2025-06-22T00:00:00.000Z'
        }
    ]

    // === Chargement ===
    async function loadData() {
        isLoading.value = true
        errorMsg.value = ''
        try {
            const res = await fetch('/api/employe')
            if (!res.ok) throw new Error('Erreur chargement')
            const json = await res.json()
            const items = json.items || json || []
            data.value = items
            computeCounts()
        } catch {
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
        counts.value.ACTIF = data.value.filter(x => x.statut === 'ACTIF').length
        counts.value.INACTIF = data.value.filter(x => x.statut === 'INACTIF').length
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
        selectedEmploye.value = null
        showForm.value = true
    }

    function openEdit(employe) {
        formMode.value = 'edit'
        selectedEmploye.value = employe
        showForm.value = true
    }

    function onView(employe) {
        detailFields.value = [
            { label: 'Nom complet', value: `${employe.nom} ${employe.prenom}` },
            { label: 'Email', value: employe.email },
            { label: 'Téléphone', value: employe.telephone },
            { label: 'Fonction', value: employe.fonction },
            { label: 'Statut', value: employe.statut },
            { label: 'Nb véhicules', value: employe.vehicules?.length ?? 0 },
            { label: 'Date rattachement', value: new Date(employe.dateRattachement).toLocaleDateString('fr-FR') }
        ]
        showDetail.value = true
    }

    // === Sauvegarde ===
    async function onFormSave(payload) {
        toastError.value = ''
        try {
            let res
            if (formMode.value === 'create') {
                res = await fetch('/api/employe', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                })
            } else {
                res = await fetch(`/api/employe/${selectedEmploye.value.id}`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                })
            }
            if (!res.ok) {
                const json = await res.json()
                toastError.value = json.message ?? "L'opération a échoué."
                return
            }
            showForm.value = false
            await loadData()
        } catch {
            toastError.value = "Impossible de contacter le serveur."
        }
    }

    // === Suppression ===
    function confirmDelete(employe) {
        employeToDelete.value = employe
        showConfirmDelete.value = true
    }

    async function onDelete() {
        showConfirmDelete.value = false
        toastError.value = ''
        try {
            const res = await fetch(`/api/employe/${employeToDelete.value.id}`, { method: 'DELETE' })
            if (!res.ok) {
                const json = await res.json()
                toastError.value = json.message ?? 'Suppression échouée.'
                return
            }
            await loadData()
        } catch {
            toastError.value = "Impossible de contacter le serveur."
        }
    }

    // === Pagination et filtrage ===
    const filtered = computed(() => {
        let list = data.value || []
        if (search.value) {
            const q = search.value.toLowerCase()
            list = list.filter(x =>
                (x.nom || '').toLowerCase().includes(q) ||
                (x.prenom || '').toLowerCase().includes(q) ||
                (x.email || '').toLowerCase().includes(q)
            )
        }
        if (filter.value !== 'Tous') {
            list = list.filter(x => x.statut === filter.value)
        }
        return list
    })

    const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / pageSize.value)))
    const paged = computed(() => {
        const start = (page.value - 1) * pageSize.value
        return filtered.value.slice(start, start + pageSize.value)
    })

    function prevPage() { if (page.value > 1) page.value-- }
    function nextPage() { if (page.value < totalPages.value) page.value++ }

    // === Lifecycle ===
    onMounted(() => {
        loadData()
        window.addEventListener('create-employe', openCreate)
    })
    onUnmounted(() => {
        window.removeEventListener('create-employe', openCreate)
    })
</script>

<style scoped>
    /* ===== STYLES IDENTIQUES AUX LISTES PRÉCÉDENTES ===== */
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
        background: rgba(0,0,0,0.06);
        padding: 0 0.5rem;
        border-radius: 20px;
        font-size: 0.7rem;
        font-weight: 600;
    }

    .filter-btn.active .count-badge {
        background: rgba(255,255,255,0.2);
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
        box-shadow: 0 4px 16px rgba(0,0,0,0.04);
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
        box-shadow: 0 8px 24px rgba(220,38,38,0.3);
        font-size: 0.9rem;
        z-index: 70;
    }
</style>