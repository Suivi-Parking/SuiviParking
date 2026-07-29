<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import api from '../services/api'

const data = ref(null)
const isLoading = ref(true)
const lastUpdated = ref(null)
let intervalId = null

async function fetchDashboard() {
  try {
    const response = await api.get('/Dashboard')
    data.value = response.data
    lastUpdated.value = new Date()
  } finally {
    isLoading.value = false
  }
}

function formatMontant(m) {
  return new Intl.NumberFormat('fr-TN', { minimumFractionDigits: 2 }).format(m) + ' DT'
}

function formatHeure(d) {
  if (!d) return ''
  return d.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
}

function tauxStyle(taux) {
  if (taux >= 90) return { chip: 'bg-red-50 text-red-600', bar: 'bg-red-500' }
  if (taux >= 70) return { chip: 'bg-amber-50 text-amber-600', bar: 'bg-amber-500' }
  return { chip: 'bg-emerald-50 text-emerald-600', bar: 'bg-emerald-500' }
}

function impayeStyle(montant) {
  if (montant === 0) return 'bg-emerald-50 text-emerald-600'
  if (montant < 500) return 'bg-amber-50 text-amber-600'
  return 'bg-red-50 text-red-600'
}

function expiresStyle(count, total) {
  if (total === 0) return 'bg-slate-100 text-slate-500'
  const ratio = count / total
  if (ratio >= 0.3) return 'bg-red-50 text-red-600'
  if (ratio > 0) return 'bg-amber-50 text-amber-600'
  return 'bg-emerald-50 text-emerald-600'
}

onMounted(() => {
  fetchDashboard()
  intervalId = setInterval(fetchDashboard, 15000)
})

onUnmounted(() => {
  clearInterval(intervalId)
})
</script>

<template>
    <div class="p-8">
        <div class="flex items-center justify-between mb-6">
            <div>
                <h1 class="text-2xl font-bold text-slate-800">Tableau de bord</h1>
                <p class="text-slate-500 text-sm mt-1">Vue d'ensemble de la plateforme</p>
            </div>
            <div class="flex items-center gap-3">
                <span v-if="lastUpdated" class="text-xs text-slate-400">
                    Mis à jour à {{ formatHeure(lastUpdated) }}
                </span>
                <button @click="fetchDashboard"
                        class="text-sm font-semibold text-primary hover:underline flex items-center gap-1">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M23 4v6h-6" />
                        <path d="M1 20v-6h6" />
                        <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15" />
                    </svg>
                    Actualiser
                </button>
            </div>
        </div>

        <div v-if="isLoading" class="text-slate-400 text-sm">Chargement du tableau de bord…</div>

        <div v-else-if="data" class="space-y-6">
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
                <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                    <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Entreprises</p>
                    <p class="text-2xl font-bold text-slate-800 mt-2">{{ data.totalEntreprises }}</p>
                    <div class="flex gap-2 mt-3 text-xs">
                        <span class="px-2 py-1 rounded-full bg-emerald-50 text-emerald-600 font-semibold">
                            {{ data.entreprisesActives }} actives
                        </span>
                        <span class="px-2 py-1 rounded-full bg-slate-100 text-slate-500 font-semibold">
                            {{ data.entreprisesInactives }} inactives
                        </span>
                    </div>
                </div>

                <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                    <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Abonnements actifs</p>
                    <p class="text-2xl font-bold text-emerald-600 mt-2">{{ data.abonnementsActifs }}</p>
                    <span class="inline-block mt-3 px-2 py-1 rounded-full text-xs font-semibold"
                          :class="expiresStyle(data.abonnementsExpires, data.abonnementsActifs + data.abonnementsExpires)">
                        {{ data.abonnementsExpires }} expirés
                    </span>
                </div>

                <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                    <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Employés</p>
                    <p class="text-2xl font-bold text-slate-800 mt-2">{{ data.totalEmployes }}</p>
                    <p class="text-xs text-slate-400 mt-3">Collaborateurs enregistrés</p>
                </div>

                <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                    <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Véhicules</p>
                    <p class="text-2xl font-bold text-slate-800 mt-2">{{ data.totalVehicules }}</p>
                    <p class="text-xs text-slate-400 mt-3">Véhicules enregistrés</p>
                </div>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-2 gap-4">
                <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                    <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Parkings</p>
                    <p class="text-2xl font-bold text-slate-800 mt-2">{{ data.totalParkings }}</p>
                    <p class="text-xs text-slate-400 mt-3">Sites gérés sur la plateforme</p>
                </div>

                <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                    <div class="flex items-center justify-between">
                        <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Taux d'occupation global</p>
                        <span class="px-2 py-1 rounded-full text-xs font-semibold"
                              :class="tauxStyle(data.tauxOccupationGlobal).chip">
                            {{ data.tauxOccupationGlobal >= 90 ? 'Saturé' : data.tauxOccupationGlobal >= 70 ? 'Élevé' : 'Normal' }}
                        </span>
                    </div>
                    <p class="text-2xl font-bold text-slate-800 mt-2">{{ data.tauxOccupationGlobal }}%</p>
                    <div class="w-full bg-slate-100 rounded-full h-2 mt-3">
                        <div class="h-2 rounded-full transition-all"
                             :class="tauxStyle(data.tauxOccupationGlobal).bar"
                             :style="{ width: Math.min(data.tauxOccupationGlobal, 100) + '%' }"></div>
                    </div>
                </div>
            </div>

            <div>
                <p class="text-sm font-semibold text-slate-600 mb-3">Revenus</p>
                <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
                    <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                        <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Revenu total</p>
                        <p class="text-2xl font-bold text-emerald-600 mt-2">{{ formatMontant(data.revenuTotal) }}</p>
                    </div>
                    <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                        <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Revenu du mois</p>
                        <p class="text-2xl font-bold text-primary mt-2">{{ formatMontant(data.revenuMensuel) }}</p>
                    </div>
                    <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
                        <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Montant impayé</p>
                        <div class="flex items-center gap-2 mt-2">
                            <p class="text-2xl font-bold" :class="impayeStyle(data.montantImpaye).split(' ')[1]">
                                {{ formatMontant(data.montantImpaye) }}
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>