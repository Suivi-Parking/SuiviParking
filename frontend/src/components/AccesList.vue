<template>
    <div class="page-container">
        <div class="page-header">
            <h1>Gestion des accès</h1>
            <p class="subtitle">Historique des entrées / sorties</p>
        </div>

        <!-- Formulaire centré (déjà stylé) -->
        <AccesForm @acces-cree="rafraichir" @acces-modifie="rafraichir" />

        <!-- Filtres -->
        <div class="filter-bar">
            <button v-for="f in filters"
                    :key="f.value"
                    class="filter-btn"
                    :class="{ active: filtres.statut === f.value }"
                    @click="setFilter(f.value)">
                {{ f.label }}
                <span class="count-badge">{{ counts[f.value] ?? 0 }}</span>
            </button>
        </div>

        <!-- États -->
        <div v-if="chargement" class="state-message">⏳ Chargement...</div>
        <div v-else-if="erreur" class="state-message error">
            ❌ {{ erreur }}
            <button class="retry-btn" @click="rafraichir">🔄 Réessayer</button>
        </div>

        <!-- Tableau -->
        <div v-else class="table-wrapper">
            <table class="data-table">
                <thead>
                    <tr>
                        <th>ID</th>
                        <th>Immatriculation</th>
                        <th>Employé</th>
                        <th>Entrée</th>
                        <th>Sortie</th>
                        <th>Durée</th>
                        <th>Statut</th>
                        <th>Motif refus</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    <AccesRow v-for="item in items"
                              :key="item.id"
                              :acces="item"
                              @enregistrer-sortie="enregistrerSortie" />
                    <tr v-if="items.length === 0">
                        <td colspan="9" class="empty-cell">📭 Aucun accès trouvé.</td>
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
    </div>
</template>

<script setup>
    import { ref, reactive, onMounted } from 'vue'
    import api from '../services/api'
    import AccesForm from './AccesForm.vue'
    import AccesRow from './AccesRow.vue'

    // === Données ===
    const items = ref([])
    const page = ref(1)
    const pageSize = ref(20)
    const totalPages = ref(1)
    const chargement = ref(false)
    const erreur = ref('')

    const filtres = reactive({ statut: '' })
    const counts = reactive({ '': 0, EN_COURS: 0, SORTI: 0, REFUSE: 0 })

    // === Filtres ===
    const filters = [
        { value: '', label: 'Tous' },
        { value: 'EN_COURS', label: 'En cours' },
        { value: 'SORTI', label: 'Sorti' },
        { value: 'REFUSE', label: 'Refusé' },
    ]

    function setFilter(val) {
        filtres.statut = val
        page.value = 1
        rafraichir()
    }

    // === Données fictives (si API indisponible) ===
    const MOCK_DATA = [
        {
            id: 13,
            vehiculeImmatriculation: 'TUN-1111',
            employeNom: 'Missaouli',
            dateHeureEntree: '2026-07-22T16:47:43',
            dateHeureSortie: null,
            dureeFormatee: null,
            statut: 'REFUSE',
            motifRefus: 'Abonnement invalide, expiré ou inactif.',
        },
        {
            id: 12,
            vehiculeImmatriculation: 'TUN-1111',
            employeNom: 'Missaouli',
            dateHeureEntree: '2026-07-21T22:51:24',
            dateHeureSortie: null,
            dureeFormatee: null,
            statut: 'REFUSE',
            motifRefus: 'Abonnement invalide, expiré ou inactif.',
        },
        {
            id: 11,
            vehiculeImmatriculation: 'TUN-1111',
            employeNom: 'Missaouli',
            dateHeureEntree: '2026-07-21T22:17:20',
            dateHeureSortie: '2026-07-21T22:51:23',
            dureeFormatee: '00h34',
            statut: 'SORTI',
            motifRefus: null,
        },
        {
            id: 10,
            vehiculeImmatriculation: 'TUN-1111',
            employeNom: 'Missaouli',
            dateHeureEntree: '2026-07-21T22:16:54',
            dateHeureSortie: null,
            dureeFormatee: null,
            statut: 'REFUSE',
            motifRefus: 'Aucun abonnement associé à la demande d\'accès.',
        },
        {
            id: 9,
            vehiculeImmatriculation: 'TUN-1111',
            employeNom: 'Missaouli',
            dateHeureEntree: '2026-07-21T15:45:04',
            dateHeureSortie: '2026-07-21T22:03:12',
            dureeFormatee: '06h18',
            statut: 'SORTI',
            motifRefus: null,
        },
    ]

    // === Chargement ===
    async function rafraichir(nouvellePage = page.value) {
        chargement.value = true
        erreur.value = ''
        page.value = nouvellePage

        try {
            const params = { page: page.value, pageSize: pageSize.value }
            if (filtres.statut) params.statut = filtres.statut

            const reponse = await api.get('/Acces', { params })
            const data = reponse.data
            items.value = data.items
            totalPages.value = data.totalPages
            // Mise à jour des compteurs (si fournis par l'API)
            if (data.counts) {
                counts[''] = data.counts.total || 0
                counts.EN_COURS = data.counts.EN_COURS || 0
                counts.SORTI = data.counts.SORTI || 0
                counts.REFUSE = data.counts.REFUSE || 0
            } else {
                // AccesController.GetHistorique ne renvoie pas de compteurs par statut
                // pour l'instant (contrairement a PaiementsController par exemple) :
                // les badges resteront a 0 tant que ce n'est pas ajoute cote backend.
            }
        } catch (e) {
            console.warn('⚠️ Backend injoignable, utilisation des données fictives', e)
            // Filtrer les données fictives selon le filtre
            let filtered = MOCK_DATA
            if (filtres.statut) {
                filtered = filtered.filter(item => item.statut === filtres.statut)
            }
            items.value = filtered
            totalPages.value = Math.ceil(filtered.length / pageSize.value)
            // Compteurs sur les données fictives
            counts[''] = MOCK_DATA.length
            counts.EN_COURS = MOCK_DATA.filter(i => i.statut === 'EN_COURS').length
            counts.SORTI = MOCK_DATA.filter(i => i.statut === 'SORTI').length
            counts.REFUSE = MOCK_DATA.filter(i => i.statut === 'REFUSE').length
            erreur.value = 'Impossible de contacter le serveur. Affichage des données de démonstration.'
        } finally {
            chargement.value = false
        }
    }

    // === Enregistrer une sortie ===
    async function enregistrerSortie(accesId) {
        try {
            await api.post('/Acces/sortie', { accesId })
            await rafraichir()
        } catch (e) {
            erreur.value = e?.response?.data?.message || e?.response?.data || "Impossible d'enregistrer la sortie."
        }
    }

    // === Lifecycle ===
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
        gap: 0.5rem;
        margin: 1.5rem 0 1.2rem;
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
</style>