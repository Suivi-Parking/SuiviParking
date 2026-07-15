<template>
  <div>
    <h1 class="text-2xl font-semibold mb-1">Véhicules</h1>
    <p class="text-sm text-muted mb-4">{{ totalCount }} véhicules enregistrés sur la plateforme</p>

    <div class="flex items-center gap-3 mb-4">
      <button v-for="f in filters" :key="f" type="button"
        @click="filter = f" :class="filter === f ? 'primary' : ''">
        {{ labels[f] }} <span class="badge">{{ counts[f] ?? 0 }}</span>
      </button>
      <input v-model="search" placeholder="Rechercher un véhicule..."
        style="margin-left:auto;padding:8px;border:1px solid #cbd5e1;border-radius:6px;" />
    </div>

    <div class="bg-panel rounded-lg shadow-sm overflow-hidden border">
      <table class="w-full">
        <thead class="text-xs uppercase text-muted bg-[var(--bg)]">
          <tr>
            <th class="p-3 text-left">Immatriculation</th>
            <th class="p-3">Marque / Modèle</th>
            <th class="p-3">Couleur</th>
            <th class="p-3">Type</th>
            <th class="p-3">Employé</th>
            <th class="p-3">Statut</th>
            <th class="p-3">Actions</th>
          </tr>
        </thead>
        <tbody>
          <template v-for="v in paged" :key="v.id">
            <VehiculeRow :vehicule="v" @edit="openEdit" @delete="confirmDelete" @view="onView" />
          </template>
        </tbody>
      </table>
    </div>

    <VehiculeForm v-if="showForm" :mode="formMode"
      :vehicule="selectedVehicule"
      @save="onFormSave"
      @cancel="() => { showForm = false }" />

    <!-- Modal confirmation suppression -->
    <ModalConfirm
      v-if="showConfirmDelete"
      title="Supprimer le véhicule"
      :message="`Êtes-vous sûr de vouloir supprimer le véhicule ${vehiculeToDelete?.immatriculation} ? Cette action est irréversible.`"
      confirmLabel="Supprimer"
      cancelLabel="Annuler"
      type="danger"
      @confirm="onDelete"
      @cancel="showConfirmDelete = false"
    />

    <!-- Modal erreur -->
    <ModalConfirm
      v-if="showError"
      title="Erreur"
      :message="errorMessage"
      confirmLabel="OK"
      :showCancel="false"
      type="warning"
      @confirm="showError = false"
      @cancel="showError = false"
    />

    <!-- Modal détails -->
    <ModalDetail
      v-if="showDetail"
      title="Détails du véhicule"
      :fields="detailFields"
      @close="showDetail = false"
    />

    <div class="mt-4" style="display:flex;justify-content:flex-end;align-items:center;gap:8px;color:#6b7280;font-size:14px;">
      <button type="button" @click="prevPage" :disabled="page<=1">Préc</button>
      <div>Page {{ page }} / {{ totalPages }}</div>
      <button type="button" @click="nextPage" :disabled="page>=totalPages">Suiv</button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import VehiculeRow from './VehiculeRow.vue'
import VehiculeForm from './VehiculeForm.vue'
import ModalConfirm from './ModalConfirm.vue'
import ModalDetail from './ModalDetail.vue'

const data = ref([])
const search = ref('')
const filter = ref('Tous')
const page = ref(1)
const pageSize = ref(10)

const filters = ['Tous', 'ACTIF', 'INACTIF']
const labels = { 'Tous': 'Tous', 'ACTIF': 'Actif', 'INACTIF': 'Inactif' }
const counts = { 'Tous': 0, 'ACTIF': 0, 'INACTIF': 0 }

// Modals
const showConfirmDelete = ref(false)
const vehiculeToDelete = ref(null)
const showError = ref(false)
const errorMessage = ref('')
const showDetail = ref(false)
const detailFields = ref([])

async function loadData() {
  try {
    const res = await fetch('/api/vehicule')
    const json = await res.json()
    if (json && json.items) {
      data.value = json.items
      counts.Tous = json.totalCount ?? data.value.length
    } else {
      data.value = json
      counts.Tous = data.value.length
    }
  } catch (e) {
    data.value = []
  }
  counts['ACTIF'] = data.value.filter(x => x.statut === 'ACTIF').length
  counts['INACTIF'] = data.value.filter(x => x.statut === 'INACTIF').length
}

onMounted(() => loadData())

function confirmDelete(vehicule) {
  vehiculeToDelete.value = vehicule
  showConfirmDelete.value = true
}

async function onDelete() {
  showConfirmDelete.value = false
  try {
    const res = await fetch(`/api/vehicule/${vehiculeToDelete.value.id}`, { method: 'DELETE' })
    if (!res.ok) {
      const json = await res.json()
      errorMessage.value = json.message ?? 'La suppression a échoué !'
      showError.value = true
      return
    }
    await loadData()
  } catch (err) {
    errorMessage.value = 'Une erreur est survenue lors de la suppression !'
    showError.value = true
  }
}

function onView(vehicule) {
  detailFields.value = [
    { label: 'Immatriculation', value: vehicule.immatriculation },
    { label: 'Marque', value: vehicule.marque },
    { label: 'Modèle', value: vehicule.modele },
    { label: 'Couleur', value: vehicule.couleur },
    { label: 'Type', value: vehicule.type },
    { label: 'Statut', value: vehicule.statut },
    { label: 'Employé', value: vehicule.employeNom ?? 'N/A' }
  ]
  showDetail.value = true
}

async function openCreate() {
  formMode.value = 'create'
  selectedVehicule.value = null
  showForm.value = true
}

onMounted(() => window.addEventListener('create-vehicule', openCreate))
onUnmounted(() => window.removeEventListener('create-vehicule', openCreate))

const showForm = ref(false)
const formMode = ref('create')
const selectedVehicule = ref(null)

async function onFormSave(payload) {
  try {
    let res
    if (formMode.value === 'create') {
      res = await fetch('/api/vehicule', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      })
    } else {
      res = await fetch(`/api/vehicule/${selectedVehicule.value?.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
      })
    }
    if (!res.ok) {
      const json = await res.json()
      errorMessage.value = json.message ?? "L'opération a échoué !"
      showError.value = true
      return
    }
    showForm.value = false
    await loadData()
  } catch (err) {
    errorMessage.value = "Une erreur est survenue !"
    showError.value = true
  }
}

function openEdit(vehicule) {
  formMode.value = 'edit'
  selectedVehicule.value = vehicule
  showForm.value = true
}

const filtered = computed(() => {
  let list = data.value || []
  if (search.value) {
    const q = search.value.toLowerCase()
    list = list.filter(x =>
      (x.immatriculation || '').toLowerCase().includes(q) ||
      (x.marque || '').toLowerCase().includes(q) ||
      (x.modele || '').toLowerCase().includes(q)
    )
  }
  if (filter.value !== 'Tous') list = list.filter(x => x.statut === filter.value)
  return list
})

const totalCount = computed(() => filtered.value.length)
const totalPages = computed(() => Math.max(1, Math.ceil(totalCount.value / pageSize.value)))
const paged = computed(() => {
  const start = (page.value - 1) * pageSize.value
  return filtered.value.slice(start, start + pageSize.value)
})

function prevPage() { if (page.value > 1) page.value-- }
function nextPage() { if (page.value < totalPages.value) page.value++ }
</script>