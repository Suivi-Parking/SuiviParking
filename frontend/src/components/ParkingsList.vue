<template>
    <div class="page-container">
        <div class="page-header">
            <h1>Parkings</h1>
            <p class="subtitle">{{ totalCount }} parking(s) enregistré(s)</p>
        </div>

        <!-- Filtres et recherche -->
        <div class="filter-bar">
            <button v-for="f in filters"
                    :key="f.value"
                    class="filter-btn"
                    :class="{ active: filterStatut === f.value }"
                    @click="setFilter(f.value)">
                {{ f.label }}
                <span class="count-badge">{{ counts[f.value] ?? 0 }}</span>
            </button>
            <div class="search-wrapper">
                <input v-model="search"
                       @keyup.enter="onSearch"
                       placeholder="Rechercher par nom, code ou adresse..."
                       class="search-input" />
            </div>
        </div>

        <!-- États -->
        <div v-if="isLoading" class="state-message">⏳ Chargement...</div>
        <div v-else-if="errorMsg" class="state-message error">
            ❌ {{ errorMsg }}
            <button class="retry-btn" @click="loadData">🔄 Réessayer</button>
        </div>

        <!-- Tableau -->
        <div v-else class="table-wrapper">
            <table class="data-table">
                <thead>
                    <tr>
                        <th>Parking</th>
                        <th>Code</th>
                        <th>Statut</th>
                        <th>Zones</th>
                        <th>Occupation</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    <ParkingRow v-for="p in data"
                                :key="p.id"
                                :parking="p"
                                @voir="openDetail(p)"
                                @modifier="openEdit(p)"
                                @supprimer="onDelete(p)" />
                    <tr v-if="data.length === 0">
                        <td colspan="6" class="empty-cell">📭 Aucun parking trouvé.</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <!-- Pagination -->
        <div class="pagination-bar">
            <button class="page-btn" :disabled="page <= 1" @click="page--; loadData()">← Précédent</button>
            <span class="page-info">Page {{ page }} / {{ totalPages }}</span>
            <button class="page-btn" :disabled="page >= totalPages" @click="page++; loadData()">Suivant →</button>
        </div>

        <!-- Modales -->
        <ParkingForm v-if="showForm"
                     :parking="editingParking"
                     @save="onSave"
                     @cancel="showForm = false" />
        <p v-if="createError" class="toast-error">{{ createError }}</p>

        <ParkingDetailModal v-if="detailParkingId"
                            :parking-id="detailParkingId"
                            @close="detailParkingId = null"
                            @changed="onDetailChanged" />
    </div>
</template>

<script setup>
    import { ref, onMounted, onUnmounted, computed } from 'vue'
    import ParkingRow from './ParkingRow.vue'
    import ParkingForm from './ParkingForm.vue'
    import ParkingDetailModal from './ParkingDetailModal.vue'

    // ===== Données =====
    const data = ref([])
    const search = ref('')
    const filterStatut = ref('Tous')
    const page = ref(1)
    const pageSize = ref(10)
    const totalCount = ref(0)
    const isLoading = ref(false)
    const errorMsg = ref('')

    const showForm = ref(false)
    const editingParking = ref(null)
    const createError = ref('')
    const detailParkingId = ref(null)

    // ===== Filtres =====
    const filters = [
        { value: 'Tous', label: 'Tous' },
        { value: 'Actif', label: 'Actif' },
        { value: 'Inactif', label: 'Inactif' },
    ]
    const counts = ref({ Tous: 0, Actif: 0, Inactif: 0 })

    function setFilter(val) {
        filterStatut.value = val
        page.value = 1
        loadData()
    }

    // ===== Données fictives (si API indisponible) =====
    const MOCK_DATA = [
        {
            id: 1,
            nom: 'Parking Centre-ville',
            code: 'PARK-CENTRE',
            adresse: '12 Rue de la République, Tunis',
            statut: 'Actif',
            nombreZones: 3,
            capaciteMax: 120,
            placesOccupees: 87,
            tauxOccupation: 72.5,
        },
        {
            id: 2,
            nom: 'Parking Gare',
            code: 'PARK-GARE',
            adresse: 'Avenue Habib Bourguiba, Tunis',
            statut: 'Actif',
            nombreZones: 2,
            capaciteMax: 80,
            placesOccupees: 65,
            tauxOccupation: 81.25,
        },
        {
            id: 3,
            nom: 'Parking Aéroport',
            code: 'PARK-AERO',
            adresse: 'Aéroport Tunis-Carthage',
            statut: 'Inactif',
            nombreZones: 4,
            capaciteMax: 200,
            placesOccupees: 0,
            tauxOccupation: 0,
        },
    ]

    // ===== Chargement =====
    async function loadData() {
        isLoading.value = true
        errorMsg.value = ''
        try {
            const params = new URLSearchParams({
                page: page.value,
                pageSize: pageSize.value,
            })
            if (search.value) params.set('search', search.value)
            if (filterStatut.value !== 'Tous') params.set('statut', filterStatut.value)

            const res = await fetch(`/api/parkings?${params.toString()}`)
            if (!res.ok) throw new Error('Requête échouée')
            const json = await res.json()

            data.value = json.items ?? []
            totalCount.value = json.totalCount ?? 0
            // Mise à jour des compteurs (si fournis par l'API)
            if (json.counts) {
                counts.value.Tous = json.counts.total || 0
                counts.value.Actif = json.counts.Actif || 0
                counts.value.Inactif = json.counts.Inactif || 0
            } else {
                // Sinon on utilise les données fictives pour le comptage (si on est en mode fallback)
                // On laisse les compteurs par défaut
            }
        } catch {
            console.warn('⚠️ Backend injoignable, utilisation des données fictives')
            // Filtrer selon le statut
            let filtered = MOCK_DATA
            if (filterStatut.value !== 'Tous') {
                filtered = filtered.filter(p => p.statut === filterStatut.value)
            }
            if (search.value) {
                const q = search.value.toLowerCase()
                filtered = filtered.filter(p =>
                    p.nom.toLowerCase().includes(q) ||
                    p.code.toLowerCase().includes(q) ||
                    p.adresse.toLowerCase().includes(q)
                )
            }
            data.value = filtered
            totalCount.value = filtered.length
            // Compteurs sur les données fictives
            counts.value.Tous = MOCK_DATA.length
            counts.value.Actif = MOCK_DATA.filter(p => p.statut === 'Actif').length
            counts.value.Inactif = MOCK_DATA.filter(p => p.statut === 'Inactif').length
            errorMsg.value = 'Impossible de contacter le serveur. Affichage des données de démonstration.'
        } finally {
            isLoading.value = false
        }
    }

    function onSearch() {
        page.value = 1
        loadData()
    }

    // ===== Création / Édition =====
    function openCreate() {
        editingParking.value = null
        showForm.value = true
        createError.value = ''
    }

    function openEdit(parking) {
        editingParking.value = parking
        showForm.value = true
        createError.value = ''
    }

    async function onSave(formData) {
        createError.value = ''
        try {
            const isEdition = !!editingParking.value
            const url = isEdition ? `/api/parkings/${editingParking.value.id}` : '/api/parkings'
            const method = isEdition ? 'PUT' : 'POST'

            const res = await fetch(url, {
                method,
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(formData),
            })
            if (!res.ok) {
                createError.value = (await res.text()) || `Erreur ${res.status}`
                return
            }
            showForm.value = false
            await loadData()
        } catch {
            createError.value = 'Impossible de contacter le serveur.'
        }
    }

    // ===== Suppression =====
    async function onDelete(parking) {
        if (!confirm(`Archiver le parking "${parking.nom}" ?`)) return
        try {
            const res = await fetch(`/api/parkings/${parking.id}`, { method: 'DELETE' })
            if (!res.ok) {
                alert((await res.text()) || "Impossible d'archiver ce parking.")
                return
            }
            await loadData()
        } catch {
            alert('Erreur réseau.')
        }
    }

    // ===== Détail =====
    function openDetail(parking) {
        detailParkingId.value = parking.id
    }

    async function onDetailChanged() {
        await loadData()
    }

    // ===== Pagination =====
    const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / pageSize.value)))
    const paged = computed(() => {
        const start = (page.value - 1) * pageSize.value
        return filtered.value.slice(start, start + pageSize.value)
    })

    function prevPage() { if (page.value > 1) page.value-- }
    function nextPage() { if (page.value < totalPages.value) page.value++ }

    // ===== Événement global pour création depuis Header =====
    function handleCreateEvent() {
        openCreate()
    }

    onMounted(() => {
        loadData()
        window.addEventListener('create-parking', handleCreateEvent)
    })
    onUnmounted(() => {
        window.removeEventListener('create-parking', handleCreateEvent)
    })
</script>

<style scoped>
    /* ===== MÊMES STYLES QUE LES AUTRES LISTES ===== */
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
        min-width: 250px;
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