<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import api from '../services/api'

const data = ref([])
const selectedParkingId = ref(null)
const isLoading = ref(true)
const lastUpdated = ref(null)
let intervalId = null

const parkingActuel = computed(() => {
  if (!selectedParkingId.value) return data.value[0] ?? null
  return data.value.find((p) => p.parkingId === selectedParkingId.value) ?? null
})

async function fetchOccupation() {
  try {
    const response = await api.get('/Occupation')
    data.value = response.data
    if (!selectedParkingId.value && data.value.length > 0) {
      selectedParkingId.value = data.value[0].parkingId
    }
    lastUpdated.value = new Date()
  } finally {
    isLoading.value = false
  }
}

function formatJour(dateStr) {
  const d = new Date(dateStr)
  return d.toLocaleDateString('fr-FR', { weekday: 'short', day: '2-digit', month: '2-digit' })
}

function formatHeure(d) {
  if (!d) return ''
  return d.toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
}

function couleurBarre(taux) {
  if (taux > 85) return 'bg-red-500'
  if (taux >= 60) return 'bg-amber-500'
  return 'bg-emerald-500'
}

function couleurTexte(taux) {
  if (taux > 85) return 'text-red-600'
  if (taux >= 60) return 'text-amber-600'
  return 'text-emerald-600'
}

function libelleNiveau(taux) {
  if (taux > 85) return 'Saturé'
  if (taux >= 60) return 'Élevé'
  return 'Normal'
}

onMounted(() => {
  fetchOccupation()
  intervalId = setInterval(fetchOccupation, 5 * 60 * 1000) // 5 minutes
})

onUnmounted(() => {
  clearInterval(intervalId)
})
</script>

<template>
    <div class="p-8">
        <div class="flex items-center justify-between mb-6">
            <div>
                <h1 class="text-2xl font-bold text-slate-800">Taux d'occupation</h1>
                <p class="text-slate-500 text-sm mt-1">Tendances d'utilisation sur les 7 derniers jours</p>
            </div>
            <div class="flex items-center gap-3">
                <span v-if="lastUpdated" class="text-xs text-slate-400">
                    Mis à jour à {{ formatHeure(lastUpdated) }}
                </span>
                <button @click="fetchOccupation"
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

        <div v-if="isLoading" class="text-slate-400 text-sm">Chargement…</div>

        <div v-else-if="data.length === 0" class="text-slate-400 text-sm">
            Aucun parking trouvé.
        </div>

        <div v-else class="space-y-6">
            <!-- Filtrage par parking -->
            <div class="flex flex-wrap gap-2">
                <button v-for="p in data"
                        :key="p.parkingId"
                        @click="selectedParkingId = p.parkingId"
                        class="px-4 py-2 rounded-lg text-sm font-semibold transition"
                        :class="selectedParkingId === p.parkingId
            ? 'bg-primary text-white shadow-soft'
            : 'bg-panel text-slate-600 border border-slate-200 hover:bg-slate-50'">
                    {{ p.parkingNom }}
                </button>
            </div>

            <div v-if="parkingActuel" class="bg-panel rounded-2xl p-6 shadow-soft border border-slate-100">
                <!-- En-tête du graphique -->
                <div class="flex items-center justify-between mb-6">
                    <div>
                        <h2 class="text-lg font-bold text-slate-800">{{ parkingActuel.parkingNom }}</h2>
                        <p class="text-xs text-slate-400 mt-1">Capacité : {{ parkingActuel.capacite }} places</p>
                    </div>
                    <div class="flex items-center gap-4 text-xs">
                        <span class="flex items-center gap-1.5">
                            <span class="w-2.5 h-2.5 rounded-full bg-emerald-500"></span> &lt; 60%
                        </span>
                        <span class="flex items-center gap-1.5">
                            <span class="w-2.5 h-2.5 rounded-full bg-amber-500"></span> 60-85%
                        </span>
                        <span class="flex items-center gap-1.5">
                            <span class="w-2.5 h-2.5 rounded-full bg-red-500"></span> &gt; 85%
                        </span>
                    </div>
                </div>

                <!-- Graphique à barres -->
                <div class="flex items-end justify-between gap-3 h-56 px-2">
                    <div v-for="jour in parkingActuel.historique"
                         :key="jour.date"
                         class="flex-1 flex flex-col items-center gap-2 h-full justify-end">
                        <span class="text-xs font-bold" :class="couleurTexte(jour.tauxOccupation)">
                            {{ jour.tauxOccupation }}%
                        </span>
                        <div class="w-full flex-1 flex items-end bg-slate-50 rounded-lg overflow-hidden">
                            <div class="w-full rounded-lg transition-all duration-500"
                                 :class="couleurBarre(jour.tauxOccupation)"
                                 :style="{ height: Math.max(jour.tauxOccupation, 3) + '%' }"></div>
                        </div>
                        <span class="text-xs text-slate-400 capitalize">{{ formatJour(jour.date) }}</span>
                    </div>
                </div>

                <!-- Résumé du jour actuel -->
                <div class="mt-6 pt-4 border-t border-slate-100 flex items-center justify-between">
                    <span class="text-sm text-slate-500">Aujourd'hui</span>
                    <span class="px-3 py-1 rounded-full text-sm font-semibold"
                          :class="{
                          'bg-emerald-50 text-emerald-600' : parkingActuel.historique.at(-1)?.tauxOccupation < 60,
              'bg-amber-50 text-amber-600': parkingActuel.historique.at(-1)?.tauxOccupation >= 60 && parkingActuel.historique.at(-1)?.tauxOccupation <= 85,
              'bg-red-50 text-red-600': parkingActuel.historique.at(-1)?.tauxOccupation > 85,
            }"
          >
            {{ libelleNiveau(parkingActuel.historique.at(-1)?.tauxOccupation ?? 0) }}
            — {{ parkingActuel.historique.at(-1)?.tauxOccupation ?? 0 }}%
          </span>
                </div>
            </div>
        </div>
    </div>
</template>
