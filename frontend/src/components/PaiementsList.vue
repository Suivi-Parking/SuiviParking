v<script setup>
import { ref, computed, onMounted } from 'vue'
import api from '../services/api'

const paiements = ref([])
const totalCount = ref(0)
const page = ref(1)
const pageSize = 10
const filtreStatut = ref(null)
const counts = ref({ tous: 0, paye: 0, enAttente: 0, enRetard: 0, rejete: 0 })
const indicateurs = ref({ montantTotalPercu: 0, montantImpaye: 0, tauxRecouvrement: 0, repartitionParModePaiement: {} })
const isLoading = ref(false)

const isCreateModalOpen = ref(false)
const isPayModalOpen = ref(false)
const selectedPaiement = ref(null)

const createForm = ref({ abonnementId: '', montant: '', dateEcheance: '', periodeCouverte: '' })
const payForm = ref({ datePaiement: '', modePaiement: 'Virement', referenceTransaction: '' })
const formError = ref('')

const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize)))

const tabs = [
  { key: null, label: 'Tous', countKey: 'tous' },
  { key: 'Paye', label: 'Payé', countKey: 'paye' },
  { key: 'EnAttente', label: 'En attente', countKey: 'enAttente' },
  { key: 'EnRetard', label: 'En retard', countKey: 'enRetard' },
  { key: 'Rejete', label: 'Rejeté', countKey: 'rejete' },
]

function badgeClass(statut) {
  switch (statut) {
    case 'Paye': return 'bg-emerald-50 text-emerald-700'
    case 'EnAttente': return 'bg-amber-50 text-amber-700'
    case 'EnRetard': return 'bg-red-50 text-red-700'
    case 'Rejete': return 'bg-slate-200 text-slate-600'
    default: return 'bg-slate-100 text-slate-600'
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

async function fetchPaiements() {
  isLoading.value = true
  try {
    const response = await api.get('/Paiements', {
      params: { statut: filtreStatut.value, page: page.value, pageSize },
    })
    paiements.value = response.data.items
    totalCount.value = response.data.totalCount
    counts.value = {
      tous: response.data.counts.tous,
      paye: response.data.counts.paye,
      enAttente: response.data.counts.enAttente,
      enRetard: response.data.counts.enRetard,
      rejete: response.data.counts.rejete,
    }
  } finally {
    isLoading.value = false
  }
}

async function fetchIndicateurs() {
  const response = await api.get('/Paiements/indicateurs')
  indicateurs.value = response.data
}

function selectTab(key) {
  filtreStatut.value = key
  page.value = 1
  fetchPaiements()
}

function changePage(delta) {
  const next = page.value + delta
  if (next >= 1 && next <= totalPages.value) {
    page.value = next
    fetchPaiements()
  }
}

function openCreateModal() {
  createForm.value = { abonnementId: '', montant: '', dateEcheance: '', periodeCouverte: '' }
  formError.value = ''
  isCreateModalOpen.value = true
}

function openPayModal(paiement) {
  selectedPaiement.value = paiement
  payForm.value = {
    datePaiement: new Date().toISOString().slice(0, 10),
    modePaiement: 'Virement',
    referenceTransaction: '',
  }
  formError.value = ''
  isPayModalOpen.value = true
}

async function handleCreate() {
  formError.value = ''
  try {
    await api.post('/Paiements', {
      abonnementId: Number(createForm.value.abonnementId),
      montant: Number(createForm.value.montant),
      dateEcheance: new Date(createForm.value.dateEcheance).toISOString(),
      periodeCouverte: createForm.value.periodeCouverte,
    })
    isCreateModalOpen.value = false
    await Promise.all([fetchPaiements(), fetchIndicateurs()])
  } catch (error) {
    formError.value = error?.response?.data?.message || error?.response?.data || 'Une erreur est survenue.'
  }
}

async function handleMarquerPaye() {
  formError.value = ''
  try {
    await api.post(`/Paiements/${selectedPaiement.value.id}/marquer-paye`, {
      datePaiement: new Date(payForm.value.datePaiement).toISOString(),
      modePaiement: payForm.value.modePaiement,
      referenceTransaction: payForm.value.referenceTransaction,
    })
    isPayModalOpen.value = false
    await Promise.all([fetchPaiements(), fetchIndicateurs()])
  } catch (error) {
    formError.value = error?.response?.data?.message || 'Une erreur est survenue.'
  }
}

async function handleRejeter(paiement) {
  if (!confirm(`Rejeter le paiement de ${formatMontant(paiement.montant)} pour ${paiement.entrepriseNom} ?`)) return
  await api.post(`/Paiements/${paiement.id}/rejeter`, {})
  await Promise.all([fetchPaiements(), fetchIndicateurs()])
}

onMounted(() => {
  fetchPaiements()
  fetchIndicateurs()
})
</script>

<template>
  <div class="p-8">
    <!-- En-tête -->
    <div class="flex items-center justify-between mb-6">
      <div>
        <h1 class="text-2xl font-bold text-slate-800">Paiements &amp; échéances</h1>
        <p class="text-slate-500 text-sm mt-1">Suivi financier des abonnements souscrits</p>
      </div>
      <button
        @click="openCreateModal"
        class="bg-primary hover:bg-accent text-white font-semibold px-4 py-2 rounded-lg shadow-soft transition"
      >
        + Nouveau paiement
      </button>
    </div>

    <!-- Cartes indicateurs -->
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
      <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
        <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Montant perçu</p>
        <p class="text-2xl font-bold text-emerald-600 mt-2">{{ formatMontant(indicateurs.montantTotalPercu) }}</p>
      </div>
      <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
        <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Montant impayé</p>
        <p class="text-2xl font-bold text-red-500 mt-2">{{ formatMontant(indicateurs.montantImpaye) }}</p>
      </div>
      <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
        <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Taux de recouvrement</p>
        <div class="flex items-center gap-2 mt-2">
          <p class="text-2xl font-bold text-primary">{{ indicateurs.tauxRecouvrement }}%</p>
        </div>
        <div class="w-full bg-slate-100 rounded-full h-1.5 mt-2">
          <div class="bg-primary h-1.5 rounded-full transition-all" :style="{ width: indicateurs.tauxRecouvrement + '%' }"></div>
        </div>
      </div>
      <div class="bg-panel rounded-2xl p-5 shadow-soft border border-slate-100">
        <p class="text-xs uppercase tracking-wide text-slate-400 font-semibold">Échéances en retard</p>
        <p class="text-2xl font-bold text-amber-600 mt-2">{{ counts.enRetard }}</p>
      </div>
    </div>

    <!-- Onglets de filtre -->
    <div class="flex flex-wrap gap-2 mb-4">
      <button
        v-for="tab in tabs"
        :key="tab.label"
        @click="selectTab(tab.key)"
        class="px-4 py-2 rounded-lg text-sm font-semibold transition"
        :class="filtreStatut === tab.key
          ? 'bg-primary text-white shadow-soft'
          : 'bg-panel text-slate-600 border border-slate-200 hover:bg-slate-50'"
      >
        {{ tab.label }}
        <span
          class="ml-1.5 px-1.5 py-0.5 rounded-full text-xs"
          :class="filtreStatut === tab.key ? 'bg-white/20' : 'bg-slate-100'"
        >
          {{ counts[tab.countKey] }}
        </span>
      </button>
    </div>

    <!-- Tableau -->
    <div class="bg-panel rounded-2xl shadow-soft border border-slate-100 overflow-hidden">
      <table class="w-full text-sm">
        <thead>
          <tr class="border-b border-slate-100 text-left text-xs uppercase tracking-wide text-slate-400">
            <th class="px-5 py-3 font-semibold">Entreprise</th>
            <th class="px-5 py-3 font-semibold">Abonnement</th>
            <th class="px-5 py-3 font-semibold">Montant</th>
            <th class="px-5 py-3 font-semibold">Échéance</th>
            <th class="px-5 py-3 font-semibold">Période</th>
            <th class="px-5 py-3 font-semibold">Statut</th>
            <th class="px-5 py-3 font-semibold text-right">Actions</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="isLoading">
            <td colspan="7" class="px-5 py-8 text-center text-slate-400">Chargement…</td>
          </tr>
          <tr v-else-if="paiements.length === 0">
            <td colspan="7" class="px-5 py-8 text-center text-slate-400">Aucun paiement trouvé.</td>
          </tr>
          <tr
            v-for="p in paiements"
            :key="p.id"
            class="border-b border-slate-50 last:border-0 hover:bg-slate-50/60 transition"
          >
            <td class="px-5 py-3 font-medium text-slate-700">{{ p.entrepriseNom }}</td>
            <td class="px-5 py-3 text-slate-500">{{ p.typeAbonnement }}</td>
            <td class="px-5 py-3 font-semibold text-slate-700">{{ formatMontant(p.montant) }}</td>
            <td class="px-5 py-3 text-slate-500">
              {{ formatDate(p.dateEcheance) }}
              <span v-if="p.joursRetard > 0" class="block text-xs text-red-500 font-medium">
                +{{ p.joursRetard }} j de retard
              </span>
            </td>
            <td class="px-5 py-3 text-slate-500">{{ p.periodeCouverte }}</td>
            <td class="px-5 py-3">
              <span class="px-2.5 py-1 rounded-full text-xs font-semibold" :class="badgeClass(p.statut)">
                {{ badgeLabel(p.statut) }}
              </span>
            </td>
            <td class="px-5 py-3 text-right">
              <button
                v-if="p.statut === 'EnAttente' || p.statut === 'EnRetard'"
                @click="openPayModal(p)"
                class="text-primary hover:underline text-xs font-semibold mr-3"
              >
                Marquer payé
              </button>
              <button
                v-if="p.statut === 'EnAttente' || p.statut === 'EnRetard'"
                @click="handleRejeter(p)"
                class="text-red-500 hover:underline text-xs font-semibold"
              >
                Rejeter
              </button>
            </td>
          </tr>
        </tbody>
      </table>

      <!-- Pagination -->
      <div class="flex items-center justify-between px-5 py-3 border-t border-slate-100 text-sm text-slate-500">
        <button
          @click="changePage(-1)"
          :disabled="page === 1"
          class="px-3 py-1.5 rounded-lg border border-slate-200 disabled:opacity-40 hover:bg-slate-50"
        >
          Préc
        </button>
        <span>Page {{ page }} / {{ totalPages }}</span>
        <button
          @click="changePage(1)"
          :disabled="page === totalPages"
          class="px-3 py-1.5 rounded-lg border border-slate-200 disabled:opacity-40 hover:bg-slate-50"
        >
          Suiv
        </button>
      </div>
    </div>

    <!-- Modal création -->
    <div v-if="isCreateModalOpen" class="fixed inset-0 bg-slate-900/40 flex items-center justify-center z-50" @click.self="isCreateModalOpen = false">
      <div class="bg-panel rounded-2xl shadow-soft w-full max-w-md p-6">
        <h2 class="text-lg font-bold text-slate-800 mb-4">Nouvelle échéance de paiement</h2>

        <label class="block text-xs font-semibold text-slate-500 mb-1">ID de l'abonnement</label>
        <input v-model="createForm.abonnementId" type="number" class="w-full border border-slate-200 rounded-lg px-3 py-2 mb-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary" />

        <label class="block text-xs font-semibold text-slate-500 mb-1">Montant (DT)</label>
        <input v-model="createForm.montant" type="number" step="0.01" class="w-full border border-slate-200 rounded-lg px-3 py-2 mb-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary" />

        <label class="block text-xs font-semibold text-slate-500 mb-1">Date d'échéance</label>
        <input v-model="createForm.dateEcheance" type="date" class="w-full border border-slate-200 rounded-lg px-3 py-2 mb-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary" />

        <label class="block text-xs font-semibold text-slate-500 mb-1">Période couverte</label>
        <input v-model="createForm.periodeCouverte" type="text" placeholder="Ex : Août 2026" class="w-full border border-slate-200 rounded-lg px-3 py-2 mb-4 text-sm focus:outline-none focus:ring-2 focus:ring-primary" />

        <p v-if="formError" class="text-red-500 text-xs mb-3">{{ formError }}</p>

        <div class="flex justify-end gap-2">
          <button @click="isCreateModalOpen = false" class="px-4 py-2 rounded-lg text-sm font-semibold text-slate-500 hover:bg-slate-50">Annuler</button>
          <button @click="handleCreate" class="px-4 py-2 rounded-lg text-sm font-semibold bg-primary hover:bg-accent text-white">Créer</button>
        </div>
      </div>
    </div>

    <!-- Modal marquer payé -->
    <div v-if="isPayModalOpen" class="fixed inset-0 bg-slate-900/40 flex items-center justify-center z-50" @click.self="isPayModalOpen = false">
      <div class="bg-panel rounded-2xl shadow-soft w-full max-w-md p-6">
        <h2 class="text-lg font-bold text-slate-800 mb-1">Marquer comme payé</h2>
        <p class="text-sm text-slate-500 mb-4">{{ selectedPaiement?.entrepriseNom }} — {{ formatMontant(selectedPaiement?.montant) }}</p>

        <label class="block text-xs font-semibold text-slate-500 mb-1">Date de paiement</label>
        <input v-model="payForm.datePaiement" type="date" class="w-full border border-slate-200 rounded-lg px-3 py-2 mb-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary" />

        <label class="block text-xs font-semibold text-slate-500 mb-1">Mode de paiement</label>
        <select v-model="payForm.modePaiement" class="w-full border border-slate-200 rounded-lg px-3 py-2 mb-3 text-sm focus:outline-none focus:ring-2 focus:ring-primary">
          <option>Virement</option>
          <option>Cheque</option>
          <option>Carte</option>
          <option>Prelevement</option>
          <option>Especes</option>
        </select>

        <label class="block text-xs font-semibold text-slate-500 mb-1">Référence de transaction</label>
        <input v-model="payForm.referenceTransaction" type="text" class="w-full border border-slate-200 rounded-lg px-3 py-2 mb-4 text-sm focus:outline-none focus:ring-2 focus:ring-primary" />

        <p v-if="formError" class="text-red-500 text-xs mb-3">{{ formError }}</p>

        <div class="flex justify-end gap-2">
          <button @click="isPayModalOpen = false" class="px-4 py-2 rounded-lg text-sm font-semibold text-slate-500 hover:bg-slate-50">Annuler</button>
          <button @click="handleMarquerPaye" class="px-4 py-2 rounded-lg text-sm font-semibold bg-emerald-600 hover:bg-emerald-700 text-white">Confirmer le paiement</button>
        </div>
      </div>
    </div>
  </div>
</template>