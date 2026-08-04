<template>
    <div class="page-container">
        <div class="page-header">
            <h1>Tableau de bord</h1>
            <p class="subtitle">
                Vue synthétique et en temps réel de la plateforme
                <span class="live-dot" :class="{ paused: isLoading }"></span>
                <span class="refresh-label">actualisé il y a {{ secondesDepuisRefresh }}s</span>
            </p>
        </div>

        <div v-if="errorMsg" class="state-message error">
            ❌ {{ errorMsg }}
            <button class="retry-btn" @click="chargerTout">🔄 Réessayer</button>
        </div>

        <template v-else>
            <!-- Alertes de saturation : toujours visibles en haut, mises en évidence -->
            <div v-if="summary?.parkings?.alertesSaturation?.length" class="alert-banner">
                <span class="alert-icon">⚠️</span>
                <div class="alert-content">
                    <strong>{{ summary.parkings.alertesSaturation.length }} parking(s) en saturation</strong>
                    <span class="alert-detail">
                        {{ summary.parkings.alertesSaturation.map(a => `${a.parkingNom} (${a.tauxOccupation}%)`).join(', ') }}
                    </span>
                </div>
            </div>

            <!-- ===== KPI Parkings ===== -->
            <div class="kpi-grid">
                <div class="kpi-card">
                    <span class="kpi-label">Parkings actifs</span>
                    <span class="kpi-value">{{ summary?.parkings?.nombreTotalParkings ?? '—' }}</span>
                </div>
                <div class="kpi-card">
                    <span class="kpi-label">Taux d'occupation global</span>
                    <span class="kpi-value">{{ summary?.parkings?.tauxOccupationGlobal ?? 0 }}%</span>
                </div>
                <div class="kpi-card">
                    <span class="kpi-label">Parking le plus occupé</span>
                    <span class="kpi-value kpi-value-sm">{{ summary?.parkings?.parkingLePlusOccupe?.nom ?? '—' }}</span>
                    <span class="kpi-sub">{{ summary?.parkings?.parkingLePlusOccupe?.tauxOccupation ?? 0 }}%</span>
                </div>
                <div class="kpi-card">
                    <span class="kpi-label">Parking le moins occupé</span>
                    <span class="kpi-value kpi-value-sm">{{ summary?.parkings?.parkingLeMoinsOccupe?.nom ?? '—' }}</span>
                    <span class="kpi-sub">{{ summary?.parkings?.parkingLeMoinsOccupe?.tauxOccupation ?? 0 }}%</span>
                </div>
            </div>

            <!-- ===== KPI Financiers ===== -->
            <div class="kpi-grid">
                <div class="kpi-card kpi-success">
                    <span class="kpi-label">Perçu ce mois-ci</span>
                    <span class="kpi-value">{{ formatMontant(summary?.financier?.montantPercuMoisEnCours) }}</span>
                </div>
                <div class="kpi-card kpi-success">
                    <span class="kpi-label">Perçu (cumulé)</span>
                    <span class="kpi-value">{{ formatMontant(summary?.financier?.montantPercuCumule) }}</span>
                </div>
                <div class="kpi-card kpi-danger">
                    <span class="kpi-label">Total impayé</span>
                    <span class="kpi-value">{{ formatMontant(summary?.financier?.montantImpayeTotal) }}</span>
                </div>
                <div class="kpi-card kpi-danger">
                    <span class="kpi-label">Paiements en retard</span>
                    <span class="kpi-value">{{ summary?.financier?.nombrePaiementsEnRetard ?? 0 }}</span>
                </div>
                <div class="kpi-card kpi-warning">
                    <span class="kpi-label">Abonnements expirant (30j)</span>
                    <span class="kpi-value">{{ summary?.financier?.nombreAbonnementsExpirantSous30Jours ?? 0 }}</span>
                </div>
                <div class="kpi-card">
                    <span class="kpi-label">Revenu total (entreprises)</span>
                    <span class="kpi-value">{{ formatMontant(summary?.financier?.revenuTotalToutesEntreprises) }}</span>
                </div>
            </div>

            <!-- ===== Accès rapide ===== -->
            <div class="quick-actions">
                <button class="qa-btn" @click="allerVers('paiements', 'create-paiement')">💳 Enregistrer un paiement</button>
                <button class="qa-btn" @click="allerVers('revenus')">📊 Revenus par entreprise</button>
                <button class="qa-btn" @click="allerVers('abonnements')">🔄 Renouveler un abonnement</button>
                <button class="qa-btn" @click="allerVers('Parking')">🅿️ Gérer les parkings</button>
                <button class="qa-btn" @click="allerVers('notifications')">🔔 Voir les notifications</button>
            </div>

            <!-- ===== Graphiques ===== -->
            <div class="charts-grid">
                <div class="chart-panel">
                    <div class="chart-panel-header">
                        <h3>Occupation par parking</h3>
                    </div>
                    <canvas ref="canvasOccupationParking"></canvas>
                </div>

                <div class="chart-panel">
                    <div class="chart-panel-header">
                        <h3>Répartition des abonnements</h3>
                    </div>
                    <canvas ref="canvasRepartitionAbonnements"></canvas>
                </div>

                <div class="chart-panel chart-panel-wide">
                    <div class="chart-panel-header">
                        <h3>Évolution de l'occupation</h3>
                        <div class="chart-filters">
                            <button class="chart-filter-btn" :class="{ active: periodeEvolution === 'jour' }" @click="changerPeriodeEvolution('jour')">Aujourd'hui</button>
                            <button class="chart-filter-btn" :class="{ active: periodeEvolution === 'semaine' }" @click="changerPeriodeEvolution('semaine')">7 derniers jours</button>
                        </div>
                    </div>
                    <canvas ref="canvasEvolutionOccupation"></canvas>
                </div>

                <div class="chart-panel chart-panel-wide">
                    <div class="chart-panel-header">
                        <h3>Paiements perçus par mois</h3>
                    </div>
                    <canvas ref="canvasPaiementsParMois"></canvas>
                </div>
            </div>

            <!-- ===== Revenus par entreprise (story Admin dédiée) ===== -->
            <div class="table-panel">
                <div class="chart-panel-header">
                    <h3>Revenus par entreprise</h3>
                </div>
                <div class="table-wrapper">
                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>Entreprise</th>
                                <th class="text-right">Revenu total perçu</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="r in summary?.financier?.revenusParEntreprise ?? []" :key="r.entrepriseId">
                                <td>{{ r.entrepriseNom }}</td>
                                <td class="text-right">{{ formatMontant(r.montantTotal) }}</td>
                            </tr>
                            <tr v-if="!(summary?.financier?.revenusParEntreprise?.length)">
                                <td colspan="2" class="empty-cell">📭 Aucun paiement encaissé pour l'instant.</td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>

            <!-- ===== Dernières notifications ===== -->
            <div class="table-panel">
                <div class="chart-panel-header">
                    <h3>Dernières notifications</h3>
                </div>
                <ul class="notif-list">
                    <li v-for="n in summary?.dernieresNotifications ?? []" :key="n.id" class="notif-item">
                        <span class="notif-badge" :class="notifBadgeClass(n.statut)">{{ n.type }}</span>
                        <span class="notif-sujet">{{ n.sujet }}</span>
                        <span class="notif-date">{{ formatDate(n.dateCreation) }}</span>
                    </li>
                    <li v-if="!(summary?.dernieresNotifications?.length)" class="empty-cell">📭 Aucune notification récente.</li>
                </ul>
            </div>
        </template>
    </div>
</template>

<script setup>
    import { ref, onMounted, onUnmounted, nextTick } from 'vue'
    import api from '../services/api'
    import Chart from 'chart.js/auto'

    const emit = defineEmits(['navigate'])

    // ===== État =====
    const summary = ref(null)
    const occupationParParking = ref([])
    const repartitionAbonnements = ref([])
    const evolutionOccupation = ref([])
    const paiementsParMois = ref([])
    const periodeEvolution = ref('jour')
    const isLoading = ref(false)
    const errorMsg = ref('')
    const derniereMiseAJour = ref(Date.now())
    const secondesDepuisRefresh = ref(0)

    // Rafraîchissement automatique : à défaut d'un canal WebSocket dédié côté
    // backend (voir explication fournie avec la livraison), on ré-interroge
    // périodiquement l'API pour que le tableau de bord reste "quasi temps réel".
    const INTERVALLE_RAFRAICHISSEMENT_MS = 15000
    let intervalId = null
    let tickId = null

    // ===== Chart.js =====
    const canvasOccupationParking = ref(null)
    const canvasRepartitionAbonnements = ref(null)
    const canvasEvolutionOccupation = ref(null)
    const canvasPaiementsParMois = ref(null)

    let chartOccupationParking = null
    let chartRepartitionAbonnements = null
    let chartEvolutionOccupation = null
    let chartPaiementsParMois = null

    const PALETTE = ['#106ad7', '#2b6fb6', '#38bdf8', '#2ecc71', '#f59e0b', '#ef4444', '#a855f7', '#14b8a6']

    function formatMontant(m) {
        return new Intl.NumberFormat('fr-TN', { minimumFractionDigits: 2 }).format(m ?? 0) + ' DT'
    }
    function formatDate(d) {
        if (!d) return '—'
        return new Date(d).toLocaleString('fr-FR')
    }
    function notifBadgeClass(statut) {
        switch (statut) {
            case 'Envoyee': return 'notif-badge-success'
            case 'Echouee': return 'notif-badge-danger'
            default: return 'notif-badge-neutral'
        }
    }

    function allerVers(vue, evenementCreation) {
        emit('navigate', vue)
        if (evenementCreation) {
            // Laisse le temps au composant cible de se monter et d'attacher
            // son écouteur avant de déclencher l'ouverture de la modale.
            nextTick(() => setTimeout(() => window.dispatchEvent(new CustomEvent(evenementCreation)), 50))
        }
    }

    function changerPeriodeEvolution(p) {
        if (periodeEvolution.value === p) return
        periodeEvolution.value = p
        chargerEvolutionOccupation()
    }

    // ===== Chargement des données =====
    async function chargerSummary() {
        const { data } = await api.get('/Dashboard/summary')
        summary.value = data
    }
    async function chargerOccupationParParking() {
        const { data } = await api.get('/Dashboard/occupation-par-parking')
        occupationParParking.value = data
    }
    async function chargerEvolutionOccupation() {
        const { data } = await api.get('/Dashboard/evolution-occupation', { params: { periode: periodeEvolution.value } })
        evolutionOccupation.value = data
        dessinerEvolutionOccupation()
    }
    async function chargerPaiementsParMois() {
        const { data } = await api.get('/Dashboard/paiements-par-mois', { params: { mois: 6 } })
        paiementsParMois.value = data
    }

    async function chargerTout() {
        isLoading.value = true
        errorMsg.value = ''
        try {
            await Promise.all([
                chargerSummary(),
                chargerOccupationParParking(),
                chargerEvolutionOccupation(),
                chargerPaiementsParMois()
            ])
            repartitionAbonnements.value = summary.value?.financier?.repartitionAbonnements ?? []
            derniereMiseAJour.value = Date.now()
            await nextTick()
            dessinerTousLesGraphiques()
        } catch (e) {
            errorMsg.value = e?.response?.data?.message || 'Erreur de chargement du tableau de bord.'
        } finally {
            isLoading.value = false
        }
    }

    // ===== Dessin des graphiques =====
    function detruire(chart) {
        if (chart) chart.destroy()
        return null
    }

    function dessinerTousLesGraphiques() {
        dessinerOccupationParking()
        dessinerRepartitionAbonnements()
        dessinerEvolutionOccupation()
        dessinerPaiementsParMois()
    }

    function dessinerOccupationParking() {
        if (!canvasOccupationParking.value) return
        chartOccupationParking = detruire(chartOccupationParking)
        chartOccupationParking = new Chart(canvasOccupationParking.value, {
            type: 'bar',
            data: {
                labels: occupationParParking.value.map(p => p.nom),
                datasets: [{
                    label: "Taux d'occupation (%)",
                    data: occupationParParking.value.map(p => p.tauxOccupation),
                    backgroundColor: occupationParParking.value.map(p => p.tauxOccupation >= 90 ? '#ef4444' : '#106ad7'),
                    borderRadius: 6
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true, max: 100 } }
            }
        })
    }

    function dessinerRepartitionAbonnements() {
        if (!canvasRepartitionAbonnements.value) return
        chartRepartitionAbonnements = detruire(chartRepartitionAbonnements)
        chartRepartitionAbonnements = new Chart(canvasRepartitionAbonnements.value, {
            type: 'doughnut',
            data: {
                labels: repartitionAbonnements.value.map(r => r.type),
                datasets: [{
                    data: repartitionAbonnements.value.map(r => r.nombre),
                    backgroundColor: PALETTE
                }]
            },
            options: { responsive: true, plugins: { legend: { position: 'bottom' } } }
        })
    }

    function dessinerEvolutionOccupation() {
        if (!canvasEvolutionOccupation.value) return
        chartEvolutionOccupation = detruire(chartEvolutionOccupation)
        chartEvolutionOccupation = new Chart(canvasEvolutionOccupation.value, {
            type: 'line',
            data: {
                labels: evolutionOccupation.value.map(p => p.label),
                datasets: [{
                    label: "Taux d'occupation (%)",
                    data: evolutionOccupation.value.map(p => p.tauxOccupation),
                    borderColor: '#106ad7',
                    backgroundColor: 'rgba(16,106,215,0.12)',
                    tension: 0.3,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true, max: 100 } }
            }
        })
    }

    function dessinerPaiementsParMois() {
        if (!canvasPaiementsParMois.value) return
        chartPaiementsParMois = detruire(chartPaiementsParMois)
        chartPaiementsParMois = new Chart(canvasPaiementsParMois.value, {
            type: 'bar',
            data: {
                labels: paiementsParMois.value.map(p => p.mois),
                datasets: [{
                    label: 'Montant perçu (DT)',
                    data: paiementsParMois.value.map(p => p.montantPercu),
                    backgroundColor: '#2ecc71',
                    borderRadius: 6
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } }
            }
        })
    }

    // ===== Cycle de vie =====
    onMounted(() => {
        chargerTout()
        intervalId = setInterval(chargerTout, INTERVALLE_RAFRAICHISSEMENT_MS)
        tickId = setInterval(() => {
            secondesDepuisRefresh.value = Math.floor((Date.now() - derniereMiseAJour.value) / 1000)
        }, 1000)
    })
    onUnmounted(() => {
        clearInterval(intervalId)
        clearInterval(tickId)
        detruire(chartOccupationParking)
        detruire(chartRepartitionAbonnements)
        detruire(chartEvolutionOccupation)
        detruire(chartPaiementsParMois)
    })
</script>

<style scoped>
    .page-container { padding: 1.5rem 2rem; }
    .page-header { margin-bottom: 1.5rem; }
    .page-header h1 { font-size: 1.6rem; font-weight: 700; color: #0b1a2f; margin: 0 0 0.2rem; }
    .subtitle { color: #64748b; font-size: 0.9rem; display: flex; align-items: center; gap: 0.5rem; }

    .live-dot {
        width: 8px; height: 8px; border-radius: 50%;
        background: #2ecc71; box-shadow: 0 0 0 rgba(46,204,113,0.5);
        animation: pulse 1.6s infinite;
    }
    .live-dot.paused { background: #f59e0b; animation: none; }
    @keyframes pulse {
        0% { box-shadow: 0 0 0 0 rgba(46,204,113,0.5); }
        70% { box-shadow: 0 0 0 6px rgba(46,204,113,0); }
        100% { box-shadow: 0 0 0 0 rgba(46,204,113,0); }
    }
    .refresh-label { font-size: 0.8rem; color: #94a3b8; }

    .state-message { padding: 2rem; text-align: center; color: #64748b; font-size: 1rem; }
    .state-message.error { color: #dc2626; }
    .retry-btn {
        margin-left: 1rem; padding: 0.3rem 1rem; border: 1px solid #dc2626;
        border-radius: 30px; background: #fff; color: #dc2626; cursor: pointer;
    }
    .retry-btn:hover { background: #fee2e2; }

    .alert-banner {
        display: flex; align-items: center; gap: 0.8rem;
        background: #fef2f2; border: 1px solid #fecaca; color: #991b1b;
        padding: 0.9rem 1.2rem; border-radius: 14px; margin-bottom: 1.5rem;
        box-shadow: 0 4px 16px rgba(220,38,38,0.08);
    }
    .alert-icon { font-size: 1.4rem; }
    .alert-content { display: flex; flex-direction: column; gap: 0.15rem; }
    .alert-detail { font-size: 0.85rem; color: #b91c1c; }

    .kpi-grid {
        display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
        gap: 1rem; margin-bottom: 1.5rem;
    }
    .kpi-card {
        background: #fff; border-radius: 16px; padding: 1.1rem 1.3rem;
        box-shadow: 0 4px 16px rgba(0,0,0,0.04); border: 1px solid #f1f5f9;
        display: flex; flex-direction: column; gap: 0.3rem;
    }
    .kpi-label { font-size: 0.78rem; color: #64748b; font-weight: 600; text-transform: uppercase; letter-spacing: 0.02em; }
    .kpi-value { font-size: 1.5rem; font-weight: 700; color: #0b1a2f; }
    .kpi-value-sm { font-size: 1.1rem; }
    .kpi-sub { font-size: 0.8rem; color: #64748b; }
    .kpi-success .kpi-value { color: #16a34a; }
    .kpi-danger .kpi-value { color: #dc2626; }
    .kpi-warning .kpi-value { color: #d97706; }

    .quick-actions { display: flex; flex-wrap: wrap; gap: 0.7rem; margin-bottom: 1.5rem; }
    .qa-btn {
        padding: 0.55rem 1.1rem; border-radius: 30px; border: 1px solid #e2e8f0;
        background: #fff; font-size: 0.85rem; font-weight: 500; color: #1e293b;
        cursor: pointer; transition: all 0.2s;
    }
    .qa-btn:hover { background: #106ad7; color: #fff; border-color: #106ad7; }

    .charts-grid {
        display: grid; grid-template-columns: 1fr 1fr; gap: 1.2rem; margin-bottom: 1.5rem;
    }
    .chart-panel-wide { grid-column: 1 / -1; }
    .chart-panel {
        background: #fff; border-radius: 16px; padding: 1.2rem;
        box-shadow: 0 4px 16px rgba(0,0,0,0.04); border: 1px solid #f1f5f9;
    }
    .chart-panel-header {
        display: flex; align-items: center; justify-content: space-between; margin-bottom: 0.8rem;
    }
    .chart-panel-header h3 { font-size: 1rem; font-weight: 600; color: #0b1a2f; margin: 0; }
    .chart-filters { display: flex; gap: 0.4rem; }
    .chart-filter-btn {
        padding: 0.3rem 0.8rem; border-radius: 30px; border: 1px solid #e2e8f0;
        background: #fff; font-size: 0.78rem; font-weight: 500; color: #475569; cursor: pointer;
    }
    .chart-filter-btn.active { background: #106ad7; color: #fff; border-color: #106ad7; }

    .table-panel {
        background: #fff; border-radius: 16px; padding: 1.2rem; margin-bottom: 1.5rem;
        box-shadow: 0 4px 16px rgba(0,0,0,0.04); border: 1px solid #f1f5f9;
    }
    .table-wrapper { overflow-x: auto; }
    .data-table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
    .data-table thead { background: #f8fafc; border-bottom: 2px solid #eef2f6; }
    .data-table th {
        padding: 0.7rem 0.8rem; text-align: left; font-weight: 600; font-size: 0.72rem;
        text-transform: uppercase; letter-spacing: 0.03em; color: #475569;
    }
    .data-table td { padding: 0.6rem 0.8rem; border-bottom: 1px solid #f1f5f9; }
    .text-right { text-align: right; }
    .empty-cell { text-align: center; padding: 1.5rem; color: #94a3b8; }

    .notif-list { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: 0.6rem; }
    .notif-item {
        display: flex; align-items: center; gap: 0.8rem; padding: 0.6rem 0.4rem;
        border-bottom: 1px solid #f1f5f9; font-size: 0.88rem;
    }
    .notif-badge {
        font-size: 0.7rem; font-weight: 600; padding: 0.2rem 0.6rem; border-radius: 20px; white-space: nowrap;
    }
    .notif-badge-success { background: #dcfce7; color: #16a34a; }
    .notif-badge-danger { background: #fee2e2; color: #dc2626; }
    .notif-badge-neutral { background: #f1f5f9; color: #475569; }
    .notif-sujet { flex: 1; color: #1e293b; }
    .notif-date { color: #94a3b8; font-size: 0.78rem; white-space: nowrap; }

    @media (max-width: 900px) {
        .charts-grid { grid-template-columns: 1fr; }
    }
</style>
