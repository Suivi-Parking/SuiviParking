<template>
  <div class="dashboard">
    <div class="dashboard-header">
      <h1>Tableau de bord</h1>
      <p>Vue d'ensemble de la plateforme</p>
    </div>

    <!-- Cartes KPI principales -->
    <div class="kpi-grid">
      <div class="kpi-card">
        <div class="kpi-icon" style="background:linear-gradient(135deg,#106ad7,#0b57c3);">
          <svg viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2"><path d="M3 21h18M5 21V7l4-4 4 4v14M9 21v-4"/><rect x="13" y="11" width="4" height="10"/></svg>
        </div>
        <div class="kpi-content">
          <p class="kpi-label">Entreprises</p>
          <p class="kpi-value">{{ stats?.nombreEntreprises ?? '—' }}</p>
        </div>
      </div>

      <div class="kpi-card">
        <div class="kpi-icon" style="background:linear-gradient(135deg,#22c55e,#16a34a);">
          <svg viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2"><rect x="2" y="5" width="20" height="14" rx="2"/><line x1="2" y1="10" x2="22" y2="10"/></svg>
        </div>
        <div class="kpi-content">
          <p class="kpi-label">Abonnements actifs</p>
          <p class="kpi-value" style="color:#16a34a;">{{ stats?.abonnementsActifs ?? '—' }}</p>
        </div>
      </div>

      <div class="kpi-card">
        <div class="kpi-icon" style="background:linear-gradient(135deg,#ef4444,#dc2626);">
          <svg viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
        </div>
        <div class="kpi-content">
          <p class="kpi-label">Abonnements expirés</p>
          <p class="kpi-value" style="color:#dc2626;">{{ stats?.abonnementsExpires ?? '—' }}</p>
        </div>
      </div>

      <div class="kpi-card">
        <div class="kpi-icon" :style="`background:linear-gradient(135deg,${couleurOccupation},${couleurOccupation}cc);`">
          <svg viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2"><path d="M3 3v18h18"/><path d="M18 17V9M13 17V5M8 17v-3"/></svg>
        </div>
        <div class="kpi-content">
          <p class="kpi-label">Taux d'occupation</p>
          <p class="kpi-value" :style="`color:${couleurOccupation};`">{{ stats?.tauxOccupation ?? 0 }}%</p>
        </div>
      </div>
    </div>

    <!-- Cartes secondaires -->
    <div class="secondary-grid">
      <div class="mini-card">
        <svg viewBox="0 0 24 24" fill="none" stroke="#106ad7" stroke-width="2"><circle cx="9" cy="8" r="3"/><path d="M2 20c0-3.5 3-6 7-6s7 2.5 7 6"/><path d="M16 5.5c1.7.4 3 2 3 3.9 0 1.9-1.3 3.5-3 3.9"/><path d="M22 20c0-2.8-1.8-5-4-5.7"/></svg>
        <div>
          <p class="mini-label">Employés</p>
          <p class="mini-value">{{ stats?.nombreEmployes ?? '—' }}</p>
        </div>
      </div>
      <div class="mini-card">
        <svg viewBox="0 0 24 24" fill="none" stroke="#106ad7" stroke-width="2"><path d="M5 17H3v-5l2-5h14l2 5v5h-2"/><circle cx="7" cy="17" r="2"/><circle cx="17" cy="17" r="2"/></svg>
        <div>
          <p class="mini-label">Véhicules</p>
          <p class="mini-value">{{ stats?.nombreVehicules ?? '—' }}</p>
        </div>
      </div>
      <div class="mini-card">
        <svg viewBox="0 0 24 24" fill="none" stroke="#106ad7" stroke-width="2"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
        <div>
          <p class="mini-label">Parkings</p>
          <p class="mini-value">{{ stats?.nombreParkings ?? '—' }}</p>
        </div>
      </div>
    </div>

    <!-- Statistiques complémentaires -->
    <div class="stats-grid">
      <div class="stats-card">
        <h3>Répartition des abonnements par statut</h3>

        <div v-if="chargement" class="empty-state">Chargement...</div>
        <div v-else-if="!repartitionEntries.length" class="empty-state">Aucune donnée disponible.</div>
        <div v-else class="bars">
          <div v-for="[statut, count] in repartitionEntries" :key="statut" class="bar-row">
            <div class="bar-header">
              <span>{{ statut }}</span>
              <span class="bar-count">{{ count }}</span>
            </div>
            <div class="bar-track">
              <div class="bar-fill" :style="`width:${pourcentageRepartition(count)}%;background:${couleurStatut(statut)};`"></div>
            </div>
          </div>
        </div>
      </div>

      <div class="stats-card">
        <h3>Indicateurs complémentaires</h3>
        <div class="indicator-list">
          <div class="indicator-row">
            <span>Revenu mensuel total</span>
            <span class="indicator-value">{{ formatMontant(stats?.statistiques?.revenuTotalMensuel ?? 0) }}</span>
          </div>
          <div class="indicator-row">
            <span>Employés actifs</span>
            <span class="indicator-value">{{ stats?.statistiques?.employesActifs ?? '—' }}</span>
          </div>
          <div class="indicator-row">
            <span>Véhicules actifs</span>
            <span class="indicator-value">{{ stats?.statistiques?.vehiculesActifs ?? '—' }}</span>
          </div>
        </div>
      </div>
    </div>

    <p v-if="erreur" class="erreur">{{ erreur }}</p>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'

const stats = ref(null)
const chargement = ref(false)
const erreur = ref('')

async function loadStats() {
  chargement.value = true
  erreur.value = ''
  try {
    const res = await fetch('/api/dashboard/stats')
    if (!res.ok) throw new Error('Impossible de charger les statistiques.')
    stats.value = await res.json()
  } catch (e) {
    erreur.value = e.message || 'Erreur inconnue.'
  } finally {
    chargement.value = false
  }
}

const repartitionEntries = computed(() => {
  const rep = stats.value?.statistiques?.repartitionAbonnementsParStatut
  return rep ? Object.entries(rep) : []
})

function pourcentageRepartition(count) {
  const total = repartitionEntries.value.reduce((sum, [, c]) => sum + c, 0)
  if (total === 0) return 0
  return Math.round((count / total) * 100)
}

function couleurStatut(statut) {
  const map = {
    Actif: '#22c55e',
    Expire: '#ef4444',
    Suspendu: '#f59e0b',
    Resilie: '#64748b',
  }
  return map[statut] ?? '#106ad7'
}

function formatMontant(montant) {
  return new Intl.NumberFormat('fr-TN', { style: 'currency', currency: 'TND' }).format(montant)
}

const couleurOccupation = computed(() => {
  const taux = stats.value?.tauxOccupation ?? 0
  if (taux < 60) return '#22c55e'
  if (taux <= 85) return '#f59e0b'
  return '#ef4444'
})

onMounted(() => loadStats())
</script>

<style scoped>
.dashboard {
  padding: 28px 32px;
  font-family: system-ui, sans-serif;
}

.dashboard-header {
  margin-bottom: 28px;
}
.dashboard-header h1 {
  font-size: 1.7rem;
  font-weight: 700;
  color: #0b1a2f;
  margin: 0 0 4px;
}
.dashboard-header p {
  color: #6b7280;
  font-size: 0.9rem;
  margin: 0;
}

/* ===== KPI principaux ===== */
.kpi-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 18px;
  margin-bottom: 20px;
}
.kpi-card {
  display: flex;
  align-items: center;
  gap: 16px;
  background: #fff;
  border-radius: 16px;
  padding: 22px;
  box-shadow: 0 2px 10px rgba(16, 106, 215, 0.06);
  border: 1px solid rgba(16, 106, 215, 0.06);
  transition: all 0.2s;
}
.kpi-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 8px 24px rgba(16, 106, 215, 0.12);
}
.kpi-icon {
  width: 48px;
  height: 48px;
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: 0 4px 12px rgba(0,0,0,0.12);
}
.kpi-icon svg {
  width: 24px;
  height: 24px;
}
.kpi-label {
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: #94a3b8;
  margin: 0 0 4px;
}
.kpi-value {
  font-size: 1.6rem;
  font-weight: 700;
  color: #106ad7;
  margin: 0;
}

/* ===== Mini cartes ===== */
.secondary-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 18px;
  margin-bottom: 24px;
}
.mini-card {
  display: flex;
  align-items: center;
  gap: 14px;
  background: #f8fbff;
  border: 1px solid rgba(16, 106, 215, 0.08);
  border-radius: 14px;
  padding: 18px 20px;
}
.mini-card svg {
  width: 26px;
  height: 26px;
  flex-shrink: 0;
}
.mini-label {
  font-size: 0.75rem;
  color: #64748b;
  margin: 0 0 2px;
}
.mini-value {
  font-size: 1.25rem;
  font-weight: 700;
  color: #0b1a2f;
  margin: 0;
}

/* ===== Cartes statistiques ===== */
.stats-grid {
  display: grid;
  grid-template-columns: 1.3fr 1fr;
  gap: 18px;
}
.stats-card {
  background: #fff;
  border-radius: 16px;
  padding: 24px;
  border: 1px solid rgba(16, 106, 215, 0.06);
  box-shadow: 0 2px 10px rgba(16, 106, 215, 0.05);
}
.stats-card h3 {
  font-size: 0.95rem;
  font-weight: 600;
  color: #0b1a2f;
  margin: 0 0 18px;
}

.empty-state {
  text-align: center;
  padding: 32px 0;
  color: #94a3b8;
  font-size: 0.85rem;
}

.bars {
  display: flex;
  flex-direction: column;
  gap: 14px;
}
.bar-row .bar-header {
  display: flex;
  justify-content: space-between;
  font-size: 0.8rem;
  color: #475569;
  margin-bottom: 6px;
}
.bar-count {
  font-weight: 700;
  color: #0b1a2f;
}
.bar-track {
  background: #eef2f7;
  border-radius: 99px;
  height: 8px;
  overflow: hidden;
}
.bar-fill {
  height: 100%;
  border-radius: 99px;
  transition: width 0.4s ease;
}

.indicator-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}
.indicator-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 0.85rem;
  color: #64748b;
  padding-bottom: 12px;
  border-bottom: 1px dashed #e2e8f0;
}
.indicator-row:last-child {
  border-bottom: none;
  padding-bottom: 0;
}
.indicator-value {
  font-weight: 700;
  color: #106ad7;
  font-size: 0.95rem;
}

.erreur {
  margin-top: 18px;
  color: #ef4444;
  font-size: 0.85rem;
  background: #fef2f2;
  border: 1px solid #fecaca;
  border-radius: 10px;
  padding: 10px 14px;
}
</style>