<template>
    <div class="page-container">
        <div class="page-header">
            <h1>Revenus</h1>
            <p class="subtitle">Revenus totaux et par entreprise, pour piloter la performance financière</p>
        </div>

        <!-- ===== Filtres ===== -->
        <div class="filter-bar">
            <div class="filter-group">
                <label>Du</label>
                <input type="date" v-model="dateDebut" @change="charger" />
            </div>
            <div class="filter-group">
                <label>Au</label>
                <input type="date" v-model="dateFin" @change="charger" />
            </div>
            <div class="filter-group">
                <label>Entreprise</label>
                <select v-model="entrepriseId" @change="charger">
                    <option :value="null">Toutes les entreprises</option>
                    <option v-for="e in entreprises" :key="e.id" :value="e.id">{{ e.nom }}</option>
                </select>
            </div>
            <div class="filter-presets">
                <button class="preset-btn" @click="appliquerPreset('mois')">Ce mois-ci</button>
                <button class="preset-btn" @click="appliquerPreset('annee')">Cette année</button>
                <button class="preset-btn" @click="appliquerPreset('tout')">Tout l'historique</button>
            </div>
        </div>

        <div v-if="errorMsg" class="state-message error">
            ❌ {{ errorMsg }}
            <button class="retry-btn" @click="charger">🔄 Réessayer</button>
        </div>

        <template v-else>
            <!-- ===== KPI ===== -->
            <div class="kpi-grid">
                <div class="kpi-card kpi-success">
                    <span class="kpi-label">Revenu total (période sélectionnée)</span>
                    <span class="kpi-value">{{ formatMontant(rapport?.revenuTotal) }}</span>
                </div>
                <div class="kpi-card">
                    <span class="kpi-label">Paiements comptabilisés</span>
                    <span class="kpi-value">{{ rapport?.nombrePaiements ?? 0 }}</span>
                </div>
                <div class="kpi-card">
                    <span class="kpi-label">Entreprises contributrices</span>
                    <span class="kpi-value">{{ rapport?.parEntreprise?.length ?? 0 }}</span>
                </div>
            </div>

            <!-- ===== Graphique ===== -->
            <div class="chart-panel">
                <div class="chart-panel-header">
                    <h3>Revenus par entreprise</h3>
                </div>
                <canvas ref="canvasRevenus"></canvas>
            </div>

            <!-- ===== Tableau détaillé ===== -->
            <div class="table-panel">
                <div class="chart-panel-header">
                    <h3>Détail par entreprise</h3>
                </div>
                <div class="table-wrapper">
                    <table class="data-table">
                        <thead>
                            <tr>
                                <th>Entreprise</th>
                                <th class="text-right">Paiements</th>
                                <th class="text-right">Revenu total</th>
                                <th class="text-right">Part du total</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-for="r in rapport?.parEntreprise ?? []" :key="r.entrepriseId">
                                <td>{{ r.entrepriseNom }}</td>
                                <td class="text-right">{{ r.nombrePaiements }}</td>
                                <td class="text-right">{{ formatMontant(r.montantTotal) }}</td>
                                <td class="text-right">
                                    <div class="part-cell">
                                        <div class="part-bar"><div class="part-bar-fill" :style="{ width: r.pourcentageDuTotal + '%' }"></div></div>
                                        <span>{{ r.pourcentageDuTotal }}%</span>
                                    </div>
                                </td>
                            </tr>
                            <tr v-if="!(rapport?.parEntreprise?.length)">
                                <td colspan="4" class="empty-cell">📭 Aucun revenu sur la période sélectionnée.</td>
                            </tr>
                        </tbody>
                        <tfoot v-if="rapport?.parEntreprise?.length">
                            <tr>
                                <td><strong>Total</strong></td>
                                <td class="text-right"><strong>{{ rapport.nombrePaiements }}</strong></td>
                                <td class="text-right"><strong>{{ formatMontant(rapport.revenuTotal) }}</strong></td>
                                <td class="text-right"><strong>100%</strong></td>
                            </tr>
                        </tfoot>
                    </table>
                </div>
            </div>
        </template>
    </div>
</template>

<script setup>
    import { ref, onMounted, onUnmounted, nextTick } from 'vue'
    import api from '../services/api'
    import Chart from 'chart.js/auto'

    const dateDebut = ref('')
    const dateFin = ref('')
    const entrepriseId = ref(null)

    const entreprises = ref([])
    const rapport = ref(null)
    const errorMsg = ref('')

    const canvasRevenus = ref(null)
    let chartRevenus = null

    const PALETTE = ['#106ad7', '#2b6fb6', '#38bdf8', '#2ecc71', '#f59e0b', '#ef4444', '#a855f7', '#14b8a6']

    function formatMontant(m) {
        return new Intl.NumberFormat('fr-TN', { minimumFractionDigits: 2 }).format(m ?? 0) + ' DT'
    }

    function appliquerPreset(preset) {
        const maintenant = new Date()
        const toISODate = (d) => d.toISOString().slice(0, 10)

        if (preset === 'mois') {
            dateDebut.value = toISODate(new Date(maintenant.getFullYear(), maintenant.getMonth(), 1))
            dateFin.value = toISODate(maintenant)
        } else if (preset === 'annee') {
            dateDebut.value = toISODate(new Date(maintenant.getFullYear(), 0, 1))
            dateFin.value = toISODate(maintenant)
        } else {
            dateDebut.value = ''
            dateFin.value = ''
        }
        charger()
    }

    async function chargerEntreprises() {
        const { data } = await api.get('/Entreprises', { params: { page: 1, pageSize: 200 } })
        entreprises.value = data.items ?? []
    }

    async function charger() {
        errorMsg.value = ''
        try {
            const params = {}
            if (dateDebut.value) params.dateDebut = dateDebut.value
            if (dateFin.value) params.dateFin = dateFin.value
            if (entrepriseId.value) params.entrepriseId = entrepriseId.value

            const { data } = await api.get('/Dashboard/revenus', { params })
            rapport.value = data
            await nextTick()
            dessinerGraphique()
        } catch (e) {
            errorMsg.value = e?.response?.data?.message || 'Erreur de chargement des revenus.'
        }
    }

    function dessinerGraphique() {
        if (!canvasRevenus.value) return
        if (chartRevenus) chartRevenus.destroy()

        const lignes = rapport.value?.parEntreprise ?? []
        chartRevenus = new Chart(canvasRevenus.value, {
            type: 'bar',
            data: {
                labels: lignes.map(r => r.entrepriseNom),
                datasets: [{
                    label: 'Revenu (DT)',
                    data: lignes.map(r => r.montantTotal),
                    backgroundColor: lignes.map((_, i) => PALETTE[i % PALETTE.length]),
                    borderRadius: 6
                }]
            },
            options: {
                responsive: true,
                indexAxis: 'y',
                plugins: { legend: { display: false } }
            }
        })
    }

    onMounted(async () => {
        await chargerEntreprises()
        await charger()
    })
    onUnmounted(() => {
        if (chartRevenus) chartRevenus.destroy()
    })
</script>

<style scoped>
    .page-container { padding: 1.5rem 2rem; }
    .page-header { margin-bottom: 1.2rem; }
    .page-header h1 { font-size: 1.6rem; font-weight: 700; color: #0b1a2f; margin: 0 0 0.2rem; }
    .subtitle { color: #64748b; font-size: 0.9rem; }

    .filter-bar {
        display: flex; flex-wrap: wrap; align-items: end; gap: 1rem;
        background: #fff; border: 1px solid #f1f5f9; border-radius: 16px;
        padding: 1rem 1.2rem; margin-bottom: 1.5rem; box-shadow: 0 4px 16px rgba(0,0,0,0.04);
    }
    .filter-group { display: flex; flex-direction: column; gap: 0.3rem; }
    .filter-group label { font-size: 0.72rem; font-weight: 600; text-transform: uppercase; color: #64748b; letter-spacing: 0.02em; }
    .filter-group input, .filter-group select {
        padding: 0.45rem 0.7rem; border: 1px solid #e2e8f0; border-radius: 10px; font-size: 0.85rem; min-width: 180px;
    }
    .filter-presets { display: flex; gap: 0.5rem; margin-left: auto; }
    .preset-btn {
        padding: 0.45rem 0.9rem; border-radius: 30px; border: 1px solid #e2e8f0;
        background: #fff; font-size: 0.8rem; font-weight: 500; color: #1e293b; cursor: pointer;
    }
    .preset-btn:hover { background: #106ad7; color: #fff; border-color: #106ad7; }

    .state-message { padding: 2rem; text-align: center; color: #64748b; font-size: 1rem; }
    .state-message.error { color: #dc2626; }
    .retry-btn {
        margin-left: 1rem; padding: 0.3rem 1rem; border: 1px solid #dc2626;
        border-radius: 30px; background: #fff; color: #dc2626; cursor: pointer;
    }

    .kpi-grid {
        display: grid; grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
        gap: 1rem; margin-bottom: 1.5rem;
    }
    .kpi-card {
        background: #fff; border-radius: 16px; padding: 1.1rem 1.3rem;
        box-shadow: 0 4px 16px rgba(0,0,0,0.04); border: 1px solid #f1f5f9;
        display: flex; flex-direction: column; gap: 0.3rem;
    }
    .kpi-label { font-size: 0.78rem; color: #64748b; font-weight: 600; text-transform: uppercase; letter-spacing: 0.02em; }
    .kpi-value { font-size: 1.5rem; font-weight: 700; color: #0b1a2f; }
    .kpi-success .kpi-value { color: #16a34a; }

    .chart-panel, .table-panel {
        background: #fff; border-radius: 16px; padding: 1.2rem; margin-bottom: 1.5rem;
        box-shadow: 0 4px 16px rgba(0,0,0,0.04); border: 1px solid #f1f5f9;
    }
    .chart-panel-header { margin-bottom: 0.8rem; }
    .chart-panel-header h3 { font-size: 1rem; font-weight: 600; color: #0b1a2f; margin: 0; }

    .table-wrapper { overflow-x: auto; }
    .data-table { width: 100%; border-collapse: collapse; font-size: 0.9rem; }
    .data-table thead { background: #f8fafc; border-bottom: 2px solid #eef2f6; }
    .data-table th {
        padding: 0.7rem 0.8rem; text-align: left; font-weight: 600; font-size: 0.72rem;
        text-transform: uppercase; letter-spacing: 0.03em; color: #475569;
    }
    .data-table td { padding: 0.6rem 0.8rem; border-bottom: 1px solid #f1f5f9; }
    .data-table tfoot td { border-top: 2px solid #eef2f6; border-bottom: none; padding-top: 0.8rem; }
    .text-right { text-align: right; }
    .empty-cell { text-align: center; padding: 1.5rem; color: #94a3b8; }

    .part-cell { display: flex; align-items: center; gap: 0.6rem; justify-content: flex-end; }
    .part-bar { width: 80px; height: 6px; border-radius: 6px; background: #eef2f6; overflow: hidden; }
    .part-bar-fill { height: 100%; background: #106ad7; }
</style>
