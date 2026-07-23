<template>
    <div class="page-container">
        <div class="page-header">
            <h1>Notifications</h1>
            <p class="subtitle">Historique des notifications envoyées</p>
        </div>

        <div v-if="!authStore.can(PERMISSIONS.NOTIFICATIONS_READ)" class="state-message error">
            Vous n'avez pas la permission de consulter les notifications.
        </div>

        <template v-else>
            <!-- Filtres -->
            <div class="filter-bar">
                <div class="filter-group">
                    <label class="filter-label">Type</label>
                    <select v-model="filtres.type" @change="rafraichir(1)" class="filter-select">
                        <option value="">Tous les types</option>
                        <option v-for="t in typesNotification" :key="t.value" :value="t.value">
                            {{ t.label }}
                        </option>
                    </select>
                </div>
                <div class="filter-group">
                    <label class="filter-label">Entreprise (ID)</label>
                    <input v-model.number="filtres.entrepriseId" @change="rafraichir(1)" type="number" min="1" placeholder="ID" class="filter-input" />
                </div>
                <button type="button" class="btn-filter" @click="rafraichir(1)">Filtrer</button>
            </div>

            <!-- États -->
            <div v-if="chargement" class="state-message">⏳ Chargement...</div>
            <div v-else-if="erreur" class="state-message error">
                ❌ {{ erreur }}
                <button class="retry-btn" @click="rafraichir(1)">🔄 Réessayer</button>
            </div>

            <!-- Table -->
            <div v-else class="table-wrapper">
                <table class="data-table">
                    <thead>
                        <tr>
                            <th>Type</th>
                            <th>Canal</th>
                            <th>Statut</th>
                            <th>Destinataire</th>
                            <th>Sujet</th>
                            <th>Entreprise</th>
                            <th>Créée le</th>
                            <th>Envoyée le</th>
                            <th></th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-for="n in items" :key="n.id" class="row-hover">
                            <td class="cell">{{ libelleType(n.type) }}</td>
                            <td class="cell">{{ n.canal }}</td>
                            <td class="cell">
                                <span class="badge" :class="badgeClasse(n.statut)">{{ n.statut }}</span>
                            </td>
                            <td class="cell">{{ n.destinataire }}</td>
                            <td class="cell max-w-xs truncate" :title="n.sujet">{{ n.sujet }}</td>
                            <td class="cell">{{ n.entrepriseNom || '—' }}</td>
                            <td class="cell">{{ formaterDate(n.dateCreation) }}</td>
                            <td class="cell">{{ n.dateEnvoi ? formaterDate(n.dateEnvoi) : '—' }}</td>
                            <td class="cell">
                                <button v-if="authStore.can(PERMISSIONS.NOTIFICATIONS_WRITE)"
                                        type="button"
                                        @click="renvoyer(n.id)"
                                        :disabled="renvoiEnCours === n.id"
                                        class="btn-action btn-renvoyer">
                                    {{ renvoiEnCours === n.id ? 'Envoi...' : 'Renvoyer' }}
                                </button>
                            </td>
                        </tr>
                        <tr v-if="items.length === 0">
                            <td colspan="9" class="empty-cell">📭 Aucune notification trouvée.</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <!-- Pagination -->
            <div class="pagination-bar" v-if="totalPages > 1">
                <button class="page-btn" :disabled="page <= 1" @click="rafraichir(page - 1)">← Précédent</button>
                <span class="page-info">Page {{ page }} / {{ totalPages }}</span>
                <button class="page-btn" :disabled="page >= totalPages" @click="rafraichir(page + 1)">Suivant →</button>
            </div>
        </template>
    </div>
</template>

<script setup>
    import { ref, reactive, onMounted } from 'vue'
    import api from '../services/api'
    import { authStore } from '../store/auth'
    import { PERMISSIONS } from '../services/permissions'

    // ===== Données =====
    const items = ref([])
    const page = ref(1)
    const pageSize = ref(20)
    const totalPages = ref(1)
    const chargement = ref(false)
    const erreur = ref('')
    const renvoiEnCours = ref(null)

    const filtres = reactive({
        type: '',
        entrepriseId: null,
    })

    // === Types de notification ===
    const typesNotification = [
        { value: 'RenouvellementAbonnement', label: 'Renouvellement abonnement' },
        { value: 'RappelPaiement', label: 'Rappel de paiement' },
        { value: 'AccesRefuse', label: 'Accès refusé' },
        { value: 'SaturationParking', label: 'Saturation parking' },
        { value: 'MiseAJourInfos', label: 'Mise à jour infos' },
    ]

    // === Données fictives (si API indisponible) ===
    const MOCK_DATA = [
        {
            id: 1,
            type: 'RenouvellementAbonnement',
            canal: 'Email',
            statut: 'Envoyee',
            destinataire: 'contact@alpha.com',
            sujet: 'Renouvellement abonnement - Entreprise Alpha',
            entrepriseNom: 'Entreprise Alpha',
            dateCreation: '2026-07-22T17:15:47.000Z',
            dateEnvoi: '2026-07-22T17:15:50.000Z',
        },
        {
            id: 2,
            type: 'AccesRefuse',
            canal: 'Email',
            statut: 'Envoyee',
            destinataire: 'admin@parkini.tn',
            sujet: 'Accès refusé - Véhicule TUN-1234',
            entrepriseNom: 'Parkini',
            dateCreation: '2026-07-22T16:48:40.000Z',
            dateEnvoi: '2026-07-22T16:48:42.000Z',
        },
        {
            id: 3,
            type: 'AccesRefuse',
            canal: 'Email',
            statut: 'EnAttente',
            destinataire: 'admin@parkini.tn',
            sujet: 'Accès refusé - Véhicule TUN-5678',
            entrepriseNom: 'Parkini',
            dateCreation: '2026-07-22T16:47:43.000Z',
            dateEnvoi: null,
        },
    ]

    // === Helpers ===
    function libelleType(type) {
        return typesNotification.find(t => t.value === type)?.label || type
    }
    function badgeClasse(statut) {
        if (statut === 'Envoyee') return 'badge-success'
        if (statut === 'Echouee') return 'badge-danger'
        return 'badge-warning' // EnAttente
    }
    function formaterDate(valeur) {
        if (!valeur) return '—'
        return new Date(valeur).toLocaleString('fr-FR')
    }

    // === Chargement ===
    async function rafraichir(nouvellePage = page.value) {
        if (!authStore.can(PERMISSIONS.NOTIFICATIONS_READ)) return

        chargement.value = true
        erreur.value = ''
        page.value = nouvellePage

        try {
            const params = { page: page.value, pageSize: pageSize.value }
            if (filtres.type) params.type = filtres.type
            if (filtres.entrepriseId) params.entrepriseId = filtres.entrepriseId

            const reponse = await api.get('/Notifications', { params })
            items.value = reponse.data.items
            totalPages.value = reponse.data.totalPages
        } catch {
            console.warn('⚠️ Backend injoignable, utilisation des données fictives')
            // Filtrer les données fictives
            let filtered = MOCK_DATA
            if (filtres.type) {
                filtered = filtered.filter(n => n.type === filtres.type)
            }
            if (filtres.entrepriseId) {
                filtered = filtered.filter(n => n.entrepriseNom?.includes(filtres.entrepriseId) || n.entrepriseId === filtres.entrepriseId)
            }
            items.value = filtered
            totalPages.value = Math.ceil(filtered.length / pageSize.value)
            erreur.value = 'Impossible de contacter le serveur. Affichage des données de démonstration.'
        } finally {
            chargement.value = false
        }
    }

    // === Renvoi ===
    async function renvoyer(id) {
        renvoiEnCours.value = id
        try {
            await api.post(`/Notifications/${id}/renvoyer`)
            await rafraichir()
        } catch (e) {
            erreur.value = e.response?.data?.message || 'Erreur lors du renvoi.'
        } finally {
            renvoiEnCours.value = null
        }
    }

    onMounted(() => rafraichir())
</script>

<style scoped>
    /* ===== STYLES IDENTIQUES AUX AUTRES LISTES ===== */
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
        align-items: flex-end;
        gap: 1rem;
        margin-bottom: 1.5rem;
    }

    .filter-group {
        display: flex;
        flex-direction: column;
    }

    .filter-label {
        font-size: 0.75rem;
        font-weight: 600;
        color: #475569;
        margin-bottom: 0.2rem;
        text-transform: uppercase;
        letter-spacing: 0.03em;
    }

    .filter-select, .filter-input {
        padding: 0.4rem 1rem;
        border: 1px solid #e2e8f0;
        border-radius: 30px;
        background: #fff;
        font-size: 0.85rem;
        min-width: 150px;
    }

        .filter-select:focus, .filter-input:focus {
            border-color: #106ad7;
            outline: none;
            box-shadow: 0 0 0 3px rgba(16, 106, 215, 0.12);
        }

    .btn-filter {
        padding: 0.4rem 1.2rem;
        border: none;
        border-radius: 30px;
        background: #106ad7;
        color: #fff;
        font-weight: 500;
        font-size: 0.85rem;
        cursor: pointer;
        transition: all 0.2s;
        box-shadow: 0 2px 8px rgba(16, 106, 215, 0.2);
    }

        .btn-filter:hover {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(16, 106, 215, 0.3);
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

    .row-hover:hover {
        background: #f8fafc;
    }

    .cell {
        padding: 0.7rem 0.6rem;
        border-bottom: 1px solid #f1f5f9;
        vertical-align: middle;
    }

    .max-w-xs {
        max-width: 200px;
    }

    .truncate {
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
    }

    .badge {
        display: inline-block;
        padding: 0.2rem 0.8rem;
        border-radius: 50px;
        font-size: 0.75rem;
        font-weight: 600;
    }

    .badge-success {
        background: #dcfce7;
        color: #15803d;
    }

    .badge-warning {
        background: #fef3c7;
        color: #b45309;
    }

    .badge-danger {
        background: #fee2e2;
        color: #b91c1c;
    }

    .btn-action {
        padding: 0.2rem 0.8rem;
        border-radius: 30px;
        border: 1px solid transparent;
        font-size: 0.75rem;
        font-weight: 500;
        cursor: pointer;
        transition: all 0.2s;
    }

    .btn-renvoyer {
        background: #dbeafe;
        color: #1d4ed8;
        border-color: #93c5fd;
    }

        .btn-renvoyer:hover {
            background: #bfdbfe;
        }

        .btn-renvoyer:disabled {
            opacity: 0.5;
            cursor: not-allowed;
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
</style>