<template>
    <div class="pl-page">
        <!-- En-tête -->
        <div class="pl-header">
            <h1>Paiements &amp; échéances</h1>
            <p class="pl-subtitle">Suivi financier des abonnements souscrits</p>
        </div>

        <!-- Cartes -->
        <div class="pl-stats">
            <div class="pl-stat">
                <div class="pl-stat-label">Montant perçu</div>
                <div class="pl-stat-value green">{{ formatMontant(indicateurs.montantTotalPercu) }}</div>
            </div>
            <div class="pl-stat">
                <div class="pl-stat-label">Montant impayé</div>
                <div class="pl-stat-value red">{{ formatMontant(indicateurs.montantImpaye) }}</div>
            </div>
            <div class="pl-stat">
                <div class="pl-stat-label">Taux de recouvrement</div>
                <div class="pl-stat-value blue">{{ indicateurs.tauxRecouvrement }}%</div>
                <div class="pl-progress"><div class="pl-progress-fill" :style="{ width: progressWidth }"></div></div>
            </div>
            <div class="pl-stat">
                <div class="pl-stat-label">Échéances en retard</div>
                <div class="pl-stat-value amber">{{ counts.enRetard }}</div>
            </div>
        </div>

        <!-- Filtres -->
        <div class="pl-filters">
            <button v-for="tab in tabs"
                    :key="tab.key"
                    class="pl-filter-btn"
                    :class="{ active: filtreStatut === tab.key }"
                    @click="selectTab(tab.key)">
                {{ tab.label }}
                <span class="pl-count">{{ counts[tab.countKey] ?? 0 }}</span>
            </button>
        </div>

        <!-- États -->
        <div v-if="isLoading" class="pl-msg">⏳ Chargement...</div>
        <div v-else-if="errorMsg" class="pl-msg error">
            ❌ {{ errorMsg }}
            <button class="pl-retry" @click="fetchPaiements">🔄 Réessayer</button>
        </div>

        <!-- Tableau -->
        <div v-else class="pl-table-wrap">
            <table class="pl-table">
                <thead>
                    <tr>
                        <th>Entreprise</th>
                        <th>Abonnement</th>
                        <th>Montant</th>
                        <th>Échéance</th>
                        <th>Période</th>
                        <th>Statut</th>
                        <th class="pl-actions-th">Actions</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="p in paiements" :key="p.id" class="pl-row">
                        <td class="pl-cell bold">{{ p.entrepriseNom }}</td>
                        <td class="pl-cell">{{ p.typeAbonnement }}</td>
                        <td class="pl-cell">{{ formatMontant(p.montant) }}</td>
                        <td class="pl-cell">
                            {{ formatDate(p.dateEcheance) }}
                            <span v-if="p.joursRetard > 0" class="pl-retard">+{{ p.joursRetard }}j</span>
                        </td>
                        <td class="pl-cell">{{ p.periodeCouverte }}</td>
                        <td class="pl-cell">
                            <span class="pl-badge" :class="badgeClass(p.statut)">{{ badgeLabel(p.statut) }}</span>
                        </td>
                        <td class="pl-cell pl-actions">
                            <button v-if="p.statut === 'EnAttente' || p.statut === 'EnRetard'"
                                    class="pl-btn-payer"
                                    @click="openPayModal(p)">
                                Marquer payé
                            </button>
                            <button v-if="p.statut === 'EnAttente' || p.statut === 'EnRetard'"
                                    class="pl-btn-rejeter"
                                    @click="handleRejeter(p)">
                                Rejeter
                            </button>
                        </td>
                    </tr>
                    <tr v-if="paiements.length === 0">
                        <td colspan="7" class="pl-empty">📭 Aucun paiement trouvé.</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <!-- Pagination -->
        <div class="pl-pagination">
            <button class="pl-page-btn" :disabled="page <= 1" @click="changePage(-1)">← Précédent</button>
            <span class="pl-page-info">Page {{ page }} / {{ totalPages }}</span>
            <button class="pl-page-btn" :disabled="page >= totalPages" @click="changePage(1)">Suivant →</button>
        </div>

        <!-- Modale création -->
        <div v-if="isCreateModalOpen" class="pl-modal-overlay" @click.self="isCreateModalOpen = false">
            <div class="pl-modal">
                <div class="pl-modal-header">
                    <h3>Nouvelle échéance</h3>
                    <button class="pl-modal-close" @click="isCreateModalOpen = false">✕</button>
                </div>
                <form @submit.prevent="handleCreate" class="pl-modal-body">
                    <div class="pl-field"><label>ID Abonnement</label><input v-model="createForm.abonnementId" type="number" required /></div>
                    <div class="pl-field"><label>Montant (DT)</label><input v-model="createForm.montant" type="number" step="0.01" required /></div>
                    <div class="pl-field"><label>Date échéance</label><input v-model="createForm.dateEcheance" type="date" required /></div>
                    <div class="pl-field"><label>Période couverte</label><input v-model="createForm.periodeCouverte" placeholder="Ex: Août 2026" required /></div>
                    <p v-if="formError" class="pl-error">{{ formError }}</p>
                    <div class="pl-modal-actions">
                        <button type="button" class="pl-btn-cancel" @click="isCreateModalOpen = false">Annuler</button>
                        <button type="submit" class="pl-btn-submit">Créer</button>
                    </div>
                </form>
            </div>
        </div>

        <!-- Modale paiement -->
        <div v-if="isPayModalOpen" class="pl-modal-overlay" @click.self="isPayModalOpen = false">
            <div class="pl-modal">
                <div class="pl-modal-header">
                    <h3>Marquer payé</h3>
                    <button class="pl-modal-close" @click="isPayModalOpen = false">✕</button>
                </div>
                <div class="pl-modal-body">
                    <p class="pl-info">{{ selectedPaiement?.entrepriseNom }} — {{ formatMontant(selectedPaiement?.montant) }}</p>
                    <form @submit.prevent="handleMarquerPaye">
                        <div class="pl-field"><label>Date paiement</label><input v-model="payForm.datePaiement" type="date" required /></div>
                        <div class="pl-field">
                            <label>Mode</label>
                            <select v-model="payForm.modePaiement">
                                <option>Virement</option>
                                <option>Chèque</option>
                                <option>Carte</option>
                                <option>Prélèvement</option>
                                <option>Espèces</option>
                            </select>
                        </div>
                        <div class="pl-field"><label>Référence</label><input v-model="payForm.referenceTransaction" /></div>
                        <p v-if="formError" class="pl-error">{{ formError }}</p>
                        <div class="pl-modal-actions">
                            <button type="button" class="pl-btn-cancel" @click="isPayModalOpen = false">Annuler</button>
                            <button type="submit" class="pl-btn-submit green">Confirmer</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>

        <!-- Toast -->
        <div v-if="toastError" class="pl-toast">{{ toastError }}</div>
    </div>
</template>

<script setup>
    import { ref, computed, onMounted, onUnmounted } from 'vue'
    // import api from '../services/api' // Décommentez si vous utilisez l'API

    // ===== Données =====
    const paiements = ref([])
    const totalCount = ref(0)
    const page = ref(1)
    const pageSize = 10
    const filtreStatut = ref(null)
    const isLoading = ref(false)
    const errorMsg = ref('')
    const toastError = ref('')

    const counts = ref({ tous: 0, paye: 0, enAttente: 0, enRetard: 0, rejete: 0 })
    const indicateurs = ref({ montantTotalPercu: 0, montantImpaye: 0, tauxRecouvrement: 0 })

    // === Modales ===
    const isCreateModalOpen = ref(false)
    const isPayModalOpen = ref(false)
    const selectedPaiement = ref(null)
    const formError = ref('')
    const createForm = ref({ abonnementId: '', montant: '', dateEcheance: '', periodeCouverte: '' })
    const payForm = ref({ datePaiement: '', modePaiement: 'Virement', referenceTransaction: '' })

    // === Données fictives (démonstration) ===
    const MOCK_DATA = [
        { id: 1, entrepriseNom: 'jasser missaoui', typeAbonnement: 'Mensuel', montant: 0.69, dateEcheance: '2026-07-22', joursRetard: 0, periodeCouverte: 'Juillet 2026', statut: 'Rejete' },
        { id: 2, entrepriseNom: 'jasser missaoui', typeAbonnement: 'Mensuel', montant: 0.13, dateEcheance: '2026-07-22', joursRetard: 1, periodeCouverte: 'Juillet 2026', statut: 'EnRetard' },
        { id: 3, entrepriseNom: 'jasser missaoui', typeAbonnement: 'Trimestriel', montant: 200, dateEcheance: '2026-07-23', joursRetard: 0, periodeCouverte: 'Août 2026', statut: 'Rejete' },
    ]

    const tabs = [
        { key: null, label: 'Tous', countKey: 'tous' },
        { key: 'Paye', label: 'Payé', countKey: 'paye' },
        { key: 'EnAttente', label: 'En attente', countKey: 'enAttente' },
        { key: 'EnRetard', label: 'En retard', countKey: 'enRetard' },
        { key: 'Rejete', label: 'Rejeté', countKey: 'rejete' },
    ]

    // === Helpers ===
    function badgeClass(statut) {
        switch (statut) {
            case 'Paye': return 'pl-badge-success'
            case 'EnAttente': return 'pl-badge-warning'
            case 'EnRetard': return 'pl-badge-danger'
            case 'Rejete': return 'pl-badge-neutral'
            default: return 'pl-badge-neutral'
        }
    }
    function badgeLabel(statut) {
        switch (statut) {
            case 'Paye': return 'Payé'
            case 'EnAttente': return 'En attente'
            case 'EnRetard': return 'En retard'
            case 'Rejete': return 'Rejeté'
            default: return statut
        }
    }
    function formatMontant(m) {
        return new Intl.NumberFormat('fr-TN', { minimumFractionDigits: 2 }).format(m) + ' DT'
    }
    function formatDate(d) {
        if (!d) return '—'
        return new Date(d).toLocaleDateString('fr-FR')
    }

    // === Chargement (API ou mock) ===
    async function fetchPaiements() {
        isLoading.value = true
        errorMsg.value = ''
        try {
            // En production, utilisez l'appel API :
            // const response = await api.get('/Paiements', { params: { statut: filtreStatut.value, page: page.value, pageSize } })
            // paiements.value = response.data.items; totalCount.value = response.data.totalCount; ...
            // Pour la démo, on utilise les données fictives
            let filtered = MOCK_DATA
            if (filtreStatut.value) filtered = filtered.filter(p => p.statut === filtreStatut.value)
            paiements.value = filtered
            totalCount.value = filtered.length
            counts.value = {
                tous: MOCK_DATA.length,
                paye: MOCK_DATA.filter(p => p.statut === 'Paye').length,
                enAttente: MOCK_DATA.filter(p => p.statut === 'EnAttente').length,
                enRetard: MOCK_DATA.filter(p => p.statut === 'EnRetard').length,
                rejete: MOCK_DATA.filter(p => p.statut === 'Rejete').length,
            }
            if (filtered.length === 0 && MOCK_DATA.length > 0) errorMsg.value = 'Aucun résultat pour ce filtre.'
        } catch {
            errorMsg.value = 'Erreur de chargement des paiements.'
        } finally {
            isLoading.value = false
        }
    }

    async function fetchIndicateurs() {
        // En production, utilisez l'appel API :
        // const response = await api.get('/Paiements/indicateurs')
        // indicateurs.value = response.data
        // Pour la démo, on calcule à partir des données fictives (au lieu d'un taux figé)
        const percu = MOCK_DATA.filter(p => p.statut === 'Paye').reduce((sum, p) => sum + p.montant, 0)
        const impaye = MOCK_DATA.filter(p => p.statut === 'EnAttente' || p.statut === 'EnRetard').reduce((sum, p) => sum + p.montant, 0)
        const total = percu + impaye
        const taux = total > 0 ? Math.round((percu / total) * 100) : 0
        indicateurs.value = { montantTotalPercu: percu, montantImpaye: impaye, tauxRecouvrement: taux }
    }

    // Largeur de la barre, toujours bornée entre 0 et 100 même si la donnée source est incohérente
    const progressWidth = computed(() => `${Math.min(100, Math.max(0, indicateurs.value.tauxRecouvrement))}%`)

    // === Filtres et pagination ===
    function selectTab(key) { filtreStatut.value = key; page.value = 1; fetchPaiements() }
    function changePage(delta) {
        const next = page.value + delta
        if (next >= 1 && next <= totalPages.value) { page.value = next; fetchPaiements() }
    }
    const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize)))

    // === Création ===
    function openCreateModal() {
        createForm.value = { abonnementId: '', montant: '', dateEcheance: '', periodeCouverte: '' }
        formError.value = ''
        isCreateModalOpen.value = true
    }
    async function handleCreate() {
        formError.value = ''
        try {
            // await api.post('/Paiements', { ... })
            // Simulation
            await new Promise(resolve => setTimeout(resolve, 500))
            isCreateModalOpen.value = false
            await fetchPaiements()
        } catch { formError.value = 'Erreur lors de la création.' }
    }

    // === Marquer payé ===
    function openPayModal(p) { selectedPaiement.value = p; payForm.value = { datePaiement: new Date().toISOString().slice(0, 10), modePaiement: 'Virement', referenceTransaction: '' }; formError.value = ''; isPayModalOpen.value = true }
    async function handleMarquerPaye() {
        formError.value = ''
        try {
            // await api.post(`/Paiements/${selectedPaiement.value.id}/marquer-paye`, { ... })
            await new Promise(resolve => setTimeout(resolve, 500))
            isPayModalOpen.value = false
            await fetchPaiements()
        } catch { formError.value = 'Erreur lors du paiement.' }
    }

    // === Rejeter ===
    async function handleRejeter(p) {
        if (!confirm(`Rejeter le paiement de ${formatMontant(p.montant)} ?`)) return
        try {
            // await api.post(`/Paiements/${p.id}/rejeter`, {})
            await new Promise(resolve => setTimeout(resolve, 300))
            await fetchPaiements()
        } catch { toastError.value = 'Erreur lors du rejet.' }
    }

    // === Événement global pour le Header ===
    function handleCreateEvent() { openCreateModal() }

    onMounted(() => {
        fetchPaiements()
        fetchIndicateurs()
        window.addEventListener('create-paiement', handleCreateEvent)
    })
    onUnmounted(() => {
        window.removeEventListener('create-paiement', handleCreateEvent)
    })
</script>

<style scoped>
    /* ===== CONTENEUR PRINCIPAL ===== */
    .pl-page {
        padding: 1.5rem 2rem;
        background: #f1f5f9;
        font-family: system-ui, -apple-system, sans-serif;
        min-height: 100vh;
    }

    /* ===== EN-TÊTE ===== */
    .pl-header {
        margin-bottom: 1.5rem;
    }

        .pl-header h1 {
            font-size: 1.6rem;
            font-weight: 700;
            color: #0b1a2f;
            margin: 0 0 0.2rem;
        }

    .pl-subtitle {
        color: #64748b;
        font-size: 0.9rem;
        margin: 0;
    }

    /* ===== CARTES ===== */
    .pl-stats {
        display: flex;
        flex-wrap: wrap;
        gap: 1rem;
        margin-bottom: 1.5rem;
    }

    .pl-stat {
        flex: 1 1 180px;
        background: #ffffff;
        border-radius: 16px;
        padding: 1rem 1.2rem;
        border: 1px solid #eef2f6;
        box-shadow: 0 2px 8px rgba(0,0,0,0.02);
    }

    .pl-stat-label {
        font-size: 0.75rem;
        text-transform: uppercase;
        letter-spacing: 0.03em;
        color: #94a3b8;
        margin-bottom: 0.2rem;
    }

    .pl-stat-value {
        font-size: 1.5rem;
        font-weight: 700;
    }

        .pl-stat-value.green {
            color: #22c55e;
        }

        .pl-stat-value.red {
            color: #ef4444;
        }

        .pl-stat-value.blue {
            color: #106ad7;
        }

        .pl-stat-value.amber {
            color: #eab308;
        }

    .pl-progress {
        width: 100%;
        height: 4px;
        background: #e2e8f0;
        border-radius: 999px;
        margin-top: 0.5rem;
        overflow: hidden;
    }

    .pl-progress-fill {
        height: 100%;
        border-radius: 999px;
        background: #106ad7;
        transition: width 0.3s;
    }

    /* ===== FILTRES ===== */
    .pl-filters {
        display: flex;
        flex-wrap: wrap;
        gap: 0.5rem;
        margin-bottom: 1.5rem;
    }

    .pl-filter-btn {
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

        .pl-filter-btn:hover {
            background: #f1f5f9;
        }

        .pl-filter-btn.active {
            background: #106ad7;
            color: #fff;
            border-color: #106ad7;
            box-shadow: 0 2px 8px rgba(16,106,215,0.25);
        }

    .pl-count {
        background: rgba(0,0,0,0.06);
        padding: 0 0.5rem;
        border-radius: 20px;
        font-size: 0.7rem;
        font-weight: 600;
    }

    .pl-filter-btn.active .pl-count {
        background: rgba(255,255,255,0.2);
        color: #fff;
    }

    /* ===== ÉTATS ===== */
    .pl-msg {
        padding: 2rem;
        text-align: center;
        color: #64748b;
    }

        .pl-msg.error {
            color: #dc2626;
        }

    .pl-retry {
        margin-left: 1rem;
        padding: 0.3rem 1rem;
        border: 1px solid #dc2626;
        border-radius: 30px;
        background: #fff;
        color: #dc2626;
        cursor: pointer;
    }

        .pl-retry:hover {
            background: #fee2e2;
        }

    /* ===== TABLEAU ===== */
    .pl-table-wrap {
        background: #fff;
        border-radius: 16px;
        box-shadow: 0 4px 16px rgba(0,0,0,0.04);
        overflow-x: auto;
        border: 1px solid #f1f5f9;
    }

    .pl-table {
        width: 100%;
        border-collapse: collapse;
        font-size: 0.9rem;
    }

        .pl-table thead {
            background: #f8fafc;
            border-bottom: 2px solid #eef2f6;
        }

        .pl-table th {
            padding: 0.8rem 0.8rem;
            text-align: left;
            font-weight: 600;
            font-size: 0.75rem;
            text-transform: uppercase;
            letter-spacing: 0.03em;
            color: #475569;
        }

    .pl-actions-th {
        text-align: right;
    }

    .pl-row:hover {
        background: #f8fafc;
    }

    .pl-cell {
        padding: 0.7rem 0.6rem;
        border-bottom: 1px solid #f1f5f9;
        vertical-align: middle;
    }

        .pl-cell.bold {
            font-weight: 600;
        }

    .pl-retard {
        display: inline-block;
        font-size: 0.7rem;
        background: #fee2e2;
        color: #b91c1c;
        padding: 0.1rem 0.5rem;
        border-radius: 20px;
        margin-left: 0.3rem;
    }

    .pl-badge {
        display: inline-block;
        padding: 0.2rem 0.8rem;
        border-radius: 50px;
        font-size: 0.75rem;
        font-weight: 600;
    }

    .pl-badge-success {
        background: #dcfce7;
        color: #15803d;
    }

    .pl-badge-warning {
        background: #fef3c7;
        color: #b45309;
    }

    .pl-badge-danger {
        background: #fee2e2;
        color: #b91c1c;
    }

    .pl-badge-neutral {
        background: #f1f5f9;
        color: #475569;
    }

    .pl-actions {
        text-align: right;
        white-space: nowrap;
    }

    .pl-btn-payer, .pl-btn-rejeter {
        padding: 0.2rem 0.8rem;
        border-radius: 30px;
        border: 1px solid transparent;
        font-size: 0.75rem;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.2s;
        margin-left: 0.3rem;
    }

    .pl-btn-payer {
        background: #dbeafe;
        color: #1d4ed8;
        border-color: #93c5fd;
    }

        .pl-btn-payer:hover {
            background: #bfdbfe;
        }

    .pl-btn-rejeter {
        background: #fee2e2;
        color: #b91c1c;
        border-color: #fca5a5;
    }

        .pl-btn-rejeter:hover {
            background: #fecaca;
        }

    .pl-empty {
        text-align: center;
        padding: 2rem;
        color: #94a3b8;
    }

    /* ===== PAGINATION ===== */
    .pl-pagination {
        display: flex;
        justify-content: center;
        align-items: center;
        gap: 1rem;
        margin-top: 1.5rem;
    }

    .pl-page-btn {
        padding: 0.4rem 1.2rem;
        border-radius: 30px;
        border: 1px solid #e2e8f0;
        background: #fff;
        font-size: 0.85rem;
        font-weight: 500;
        cursor: pointer;
    }

        .pl-page-btn:hover:not(:disabled) {
            background: #f1f5f9;
        }

        .pl-page-btn:disabled {
            opacity: 0.4;
            cursor: not-allowed;
        }

    .pl-page-info {
        font-size: 0.9rem;
        color: #475569;
    }

    /* ===== MODALES ===== */
    .pl-modal-overlay {
        position: fixed;
        inset: 0;
        display: flex;
        align-items: center;
        justify-content: center;
        background: rgba(15,23,42,0.5);
        backdrop-filter: blur(4px);
        z-index: 1000;
    }

    .pl-modal {
        width: 500px;
        max-width: 94%;
        background: #fff;
        border-radius: 20px;
        box-shadow: 0 20px 60px rgba(0,0,0,0.25);
        overflow: hidden;
    }

    .pl-modal-header {
        padding: 1.2rem 1.5rem;
        border-bottom: 1px solid #eef2f6;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

        .pl-modal-header h3 {
            margin: 0;
            font-weight: 600;
            font-size: 1.1rem;
            color: #0b1a2f;
        }

    .pl-modal-close {
        background: none;
        border: none;
        font-size: 1.3rem;
        color: #94a3b8;
        cursor: pointer;
    }

        .pl-modal-close:hover {
            color: #1e293b;
        }

    .pl-modal-body {
        padding: 1.5rem;
    }

    .pl-info {
        background: #f8fafc;
        padding: 0.5rem 1rem;
        border-radius: 10px;
        font-size: 0.9rem;
        color: #1e293b;
        margin-bottom: 1rem;
    }

    .pl-field {
        margin-bottom: 1rem;
    }

        .pl-field label {
            display: block;
            font-size: 0.85rem;
            font-weight: 500;
            color: #1e293b;
            margin-bottom: 0.3rem;
        }

        .pl-field input, .pl-field select {
            width: 100%;
            padding: 0.6rem 0.9rem;
            border: 1px solid #e2e8f0;
            border-radius: 10px;
            font-size: 0.95rem;
            background: #fafcfd;
        }

            .pl-field input:focus, .pl-field select:focus {
                border-color: #106ad7;
                outline: none;
                box-shadow: 0 0 0 4px rgba(16,106,215,0.1);
            }

    .pl-error {
        color: #dc2626;
        font-size: 0.8rem;
        margin-top: 0.2rem;
    }

    .pl-modal-actions {
        display: flex;
        justify-content: flex-end;
        gap: 0.8rem;
        margin-top: 1.2rem;
    }

    .pl-btn-cancel {
        padding: 0.5rem 1.2rem;
        border: 1px solid #e2e8f0;
        border-radius: 30px;
        background: #fff;
        color: #1e293b;
        cursor: pointer;
    }

        .pl-btn-cancel:hover {
            background: #f1f5f9;
        }

    .pl-btn-submit {
        padding: 0.5rem 1.6rem;
        border: none;
        border-radius: 30px;
        background: linear-gradient(135deg, #106ad7, #0b57c3);
        color: #fff;
        font-weight: 600;
        cursor: pointer;
        box-shadow: 0 4px 12px rgba(16,106,215,0.3);
        transition: all 0.2s;
    }

        .pl-btn-submit:hover {
            transform: translateY(-2px);
            box-shadow: 0 8px 20px rgba(16,106,215,0.4);
        }

        .pl-btn-submit.green {
            background: linear-gradient(135deg, #22c55e, #16a34a);
            box-shadow: 0 4px 12px rgba(34,197,94,0.3);
        }

            .pl-btn-submit.green:hover {
                box-shadow: 0 8px 20px rgba(34,197,94,0.4);
            }

    /* ===== TOAST ===== */
    .pl-toast {
        position: fixed;
        bottom: 1.5rem;
        right: 1.5rem;
        background: #dc2626;
        color: #fff;
        padding: 0.6rem 1.4rem;
        border-radius: 12px;
        box-shadow: 0 8px 24px rgba(220,38,38,0.3);
        font-size: 0.9rem;
        z-index: 2000;
    }

    /* ===== RESPONSIVE ===== */
    @media (max-width: 768px) {
        .pl-stats {
            flex-direction: column;
        }

        .pl-stat {
            flex: 1 1 100%;
        }

        .pl-table {
            font-size: 0.8rem;
        }

            .pl-table th, .pl-table td {
                padding: 0.4rem;
            }

        .pl-actions {
            white-space: normal;
        }

        .pl-btn-payer, .pl-btn-rejeter {
            display: inline-block;
            margin-top: 0.2rem;
        }
    }
</style>