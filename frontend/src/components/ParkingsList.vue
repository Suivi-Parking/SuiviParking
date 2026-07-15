<script setup>
import { ref, onMounted, onUnmounted, computed } from 'vue'
import ParkingRow from './ParkingRow.vue'
import ParkingForm from './ParkingForm.vue'
import ParkingDetailModal from './ParkingDetailModal.vue'

const data = ref([])
const search = ref('')
const page = ref(1)
const pageSize = ref(10)
const totalCount = ref(0)
const isLoading = ref(false)
const errorMsg = ref('')

const showForm = ref(false)
const editingParking = ref(null) // null = creation, sinon objet = edition
const createError = ref('')

const detailParkingId = ref(null)

async function loadData() {
  isLoading.value = true
  errorMsg.value = ''
  try {
    const params = new URLSearchParams({ page: page.value, pageSize: pageSize.value })
    if (search.value) params.set('search', search.value)

    const res = await fetch(`/api/parkings?${params.toString()}`)
    if (!res.ok) throw new Error('Requete echouee')
    const json = await res.json()

    data.value = json.items ?? []
    totalCount.value = json.totalCount ?? 0
  } catch (e) {
    errorMsg.value = "Impossible de charger les parkings. Verifiez que le backend tourne."
    data.value = []
  } finally {
    isLoading.value = false
  }
}

function onSearch() {
  page.value = 1
  loadData()
}

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
      body: JSON.stringify(formData)
    })

    if (!res.ok) {
      // 409 = code deja utilise, 400 = validation (ex: horaires manquants, zones invalides)
      createError.value = (await res.text()) || `Erreur ${res.status}`
      return
    }

    showForm.value = false
    await loadData()
  } catch {
    createError.value = "Impossible de contacter le serveur."
  }
}

async function onDelete(parking) {
  if (!confirm(`Archiver le parking "${parking.nom}" ?`)) return

  const res = await fetch(`/api/parkings/${parking.id}`, { method: 'DELETE' })
  if (!res.ok) {
    // Peut echouer si le parking a des acces recents (regle metier du ticket)
    alert((await res.text()) || "Impossible d'archiver ce parking.")
    return
  }
  await loadData()
}

function openDetail(parking) {
  detailParkingId.value = parking.id
}

async function onDetailChanged() {
  await loadData()
}

const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize.value)))

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

<template>
  <div class="p-6">
    <div class="flex items-center justify-between mb-4">
      <p class="text-sm text-gray-500">{{ totalCount }} parking(s) enregistré(s)</p>
      <input
        v-model="search"
        @keyup.enter="onSearch"
        placeholder="Rechercher par nom, code ou adresse..."
        class="px-4 py-2 rounded-lg border w-72"
      />
    </div>

    <p v-if="isLoading" class="text-gray-500">Chargement...</p>
    <p v-else-if="errorMsg" class="text-danger">{{ errorMsg }}</p>

    <table v-else class="w-full bg-panel rounded-xl overflow-hidden">
      <thead>
        <tr class="text-left text-xs uppercase text-gray-500 border-b">
          <th class="p-3">Parking</th>
          <th class="p-3">Code</th>
          <th class="p-3">Statut</th>
          <th class="p-3">Zones</th>
          <th class="p-3">Occupation</th>
          <th class="p-3">Actions</th>
        </tr>
      </thead>
      <tbody>
        <ParkingRow
          v-for="p in data"
          :key="p.id"
          :parking="p"
          @voir="openDetail(p)"
          @modifier="openEdit(p)"
          @supprimer="onDelete(p)"
        />
        <tr v-if="data.length === 0">
          <td colspan="6" class="text-center p-6 text-gray-400">Aucun parking trouvé.</td>
        </tr>
      </tbody>
    </table>

    <div class="flex items-center justify-center gap-4 mt-4">
      <button :disabled="page <= 1" @click="page--; loadData()">←</button>
      <span class="text-sm">Page {{ page }} / {{ totalPages }}</span>
      <button :disabled="page >= totalPages" @click="page++; loadData()">→</button>
    </div>

    <div v-if="showForm">
      <ParkingForm
        :parking="editingParking"
        @save="onSave"
        @cancel="showForm = false"
      />
      <p v-if="createError" class="fixed bottom-4 right-4 bg-danger text-white px-4 py-2 rounded-lg shadow-lg text-sm max-w-md">
        {{ createError }}
      </p>
    </div>

    <ParkingDetailModal
      v-if="detailParkingId"
      :parking-id="detailParkingId"
      @close="detailParkingId = null"
      @changed="onDetailChanged"
    />
  </div>
</template>
