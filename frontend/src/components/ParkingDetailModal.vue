<script setup>
import { ref, reactive, onMounted } from 'vue'

const props = defineProps({
  parkingId: { type: Number, required: true }
})
const emit = defineEmits(['close', 'changed'])

const parking = ref(null)
const isLoading = ref(true)
const errorMsg = ref('')

const typesZone = ['Standard', 'Visiteurs', 'Residents', 'VIP', 'Livraison', 'Autre']

const newZone = reactive({ nom: '', type: 'Standard', nombrePlaces: 0 })
const newZoneError = ref('')

const editingZoneId = ref(null)
const editZoneForm = reactive({ nom: '', type: 'Standard', nombrePlaces: 0 })
const editZoneError = ref('')

const occupationInputs = reactive({}) // { [zoneId]: valeur saisie }

async function loadDetail() {
  isLoading.value = true
  errorMsg.value = ''
  try {
    const res = await fetch(`/api/parkings/${props.parkingId}`)
    if (!res.ok) throw new Error()
    parking.value = await res.json()
    parking.value.zones.forEach(z => {
      occupationInputs[z.id] = z.placesOccupees
    })
  } catch {
    errorMsg.value = 'Impossible de charger le détail de ce parking.'
  } finally {
    isLoading.value = false
  }
}

function barColor(taux) {
  if (taux > 90) return 'bg-danger'
  if (taux >= 70) return 'bg-warning'
  return 'bg-success'
}

async function ajouterZone() {
  newZoneError.value = ''
  if (!newZone.nom || newZone.nombrePlaces <= 0) {
    newZoneError.value = 'Nom et nombre de places (> 0) requis.'
    return
  }
  const res = await fetch(`/api/parkings/${props.parkingId}/zones`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(newZone)
  })
  if (!res.ok) {
    newZoneError.value = (await res.text()) || 'Erreur lors de la création.'
    return
  }
  newZone.nom = ''
  newZone.type = 'Standard'
  newZone.nombrePlaces = 0
  await loadDetail()
  emit('changed')
}

function startEditZone(zone) {
  editingZoneId.value = zone.id
  editZoneForm.nom = zone.nom
  editZoneForm.type = zone.type
  editZoneForm.nombrePlaces = zone.nombrePlaces
  editZoneError.value = ''
}

async function saveEditZone(zoneId) {
  editZoneError.value = ''
  const res = await fetch(`/api/parkings/zones/${zoneId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(editZoneForm)
  })
  if (!res.ok) {
    editZoneError.value = (await res.text()) || 'Erreur lors de la modification.'
    return
  }
  editingZoneId.value = null
  await loadDetail()
  emit('changed')
}

async function supprimerZone(zoneId) {
  if (!confirm('Supprimer cette zone ? (impossible si c\'est la dernière du parking)')) return
  const res = await fetch(`/api/parkings/zones/${zoneId}`, { method: 'DELETE' })
  if (!res.ok) {
    alert((await res.text()) || 'Suppression impossible.')
    return
  }
  await loadDetail()
  emit('changed')
}

async function toggleActive(zone) {
  const res = await fetch(`/api/parkings/zones/${zone.id}/statut`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(!zone.estActive)
  })
  if (!res.ok) {
    alert((await res.text()) || 'Erreur lors du changement de statut.')
    return
  }
  await loadDetail()
  emit('changed')
}

async function ajusterOccupation(zone) {
  const valeur = occupationInputs[zone.id]
  const res = await fetch(`/api/parkings/zones/${zone.id}/occupation`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ placesOccupees: valeur })
  })
  if (!res.ok) {
    alert((await res.text()) || "Erreur lors de la mise à jour de l'occupation.")
    return
  }
  await loadDetail()
  emit('changed')
}

onMounted(loadDetail)
</script>

<template>
  <div class="fixed inset-0 bg-gray-900/40 flex items-center justify-center z-50 p-4">
    <div class="bg-white rounded-xl shadow-xl w-[760px] max-h-[90vh] overflow-y-auto">
      <header class="p-4 border-b flex justify-between items-center sticky top-0 bg-white">
        <div>
          <h3 class="font-semibold text-lg">{{ parking?.nom ?? 'Détail du parking' }}</h3>
          <p v-if="parking" class="text-xs text-gray-400">
            {{ parking.code }} · {{ parking.adresse }}
          </p>
        </div>
        <button type="button" class="text-gray-400 hover:text-gray-600" @click="$emit('close')">✕</button>
      </header>

      <div class="p-4">
        <p v-if="isLoading" class="text-gray-500">Chargement...</p>
        <p v-else-if="errorMsg" class="text-danger">{{ errorMsg }}</p>

        <template v-else-if="parking">
          <!-- Recapitulatif global -->
          <div class="grid grid-cols-3 gap-3 mb-6">
            <div class="bg-panel rounded-lg p-3">
              <p class="text-xs text-gray-400">Capacité totale (zones)</p>
              <p class="text-xl font-semibold">{{ parking.capaciteMax }}</p>
            </div>
            <div class="bg-panel rounded-lg p-3">
              <p class="text-xs text-gray-400">Places occupées</p>
              <p class="text-xl font-semibold">{{ parking.placesOccupees }}</p>
            </div>
            <div class="bg-panel rounded-lg p-3">
              <p class="text-xs text-gray-400">Taux d'occupation</p>
              <p class="text-xl font-semibold">{{ parking.tauxOccupation }}%</p>
            </div>
          </div>

          <!-- Liste des zones -->
          <h4 class="font-medium mb-2">Zones</h4>
          <div class="space-y-3 mb-6">
            <div
              v-for="zone in parking.zones"
              :key="zone.id"
              class="border rounded-lg p-3"
              :class="!zone.estActive ? 'opacity-50 bg-gray-50' : ''"
            >
              <template v-if="editingZoneId === zone.id">
                <div class="flex gap-2 mb-2">
                  <input v-model="editZoneForm.nom" class="flex-1 px-2 py-1 border rounded" />
                  <select v-model="editZoneForm.type" class="w-32 px-2 py-1 border rounded text-sm">
                    <option v-for="t in typesZone" :key="t" :value="t">{{ t }}</option>
                  </select>
                  <input v-model.number="editZoneForm.nombrePlaces" type="number" min="0" class="w-24 px-2 py-1 border rounded" />
                </div>
                <p v-if="editZoneError" class="text-danger text-xs mb-2">{{ editZoneError }}</p>
                <div class="flex gap-2">
                  <button class="px-3 py-1 rounded border text-xs" @click="editingZoneId = null">Annuler</button>
                  <button class="px-3 py-1 rounded bg-primary text-white text-xs" @click="saveEditZone(zone.id)">Enregistrer</button>
                </div>
              </template>

              <template v-else>
                <div class="flex justify-between items-start mb-2">
                  <div>
                    <p class="font-medium">
                      {{ zone.nom }}
                      <span class="text-xs font-normal text-gray-400">({{ zone.type }})</span>
                      <span v-if="!zone.estActive" class="text-xs font-normal text-gray-400">— désactivée</span>
                    </p>
                    <p class="text-xs text-gray-400">{{ zone.placesOccupees }}/{{ zone.nombrePlaces }} places · {{ zone.tauxOccupation }}%</p>
                  </div>
                  <div class="flex gap-2">
                    <button class="text-xs text-gray-500 hover:text-primary" @click="toggleActive(zone)">
                      {{ zone.estActive ? 'Désactiver' : 'Activer' }}
                    </button>
                    <button class="text-xs text-gray-500 hover:text-primary" @click="startEditZone(zone)">Modifier</button>
                    <button class="text-xs text-gray-500 hover:text-danger" @click="supprimerZone(zone.id)">Supprimer</button>
                  </div>
                </div>

                <div class="h-2 bg-gray-100 rounded-full overflow-hidden mb-2">
                  <div
                    class="h-full rounded-full transition-all"
                    :class="barColor(zone.tauxOccupation)"
                    :style="{ width: Math.min(zone.tauxOccupation, 100) + '%' }"
                  />
                </div>

                <div class="flex items-center gap-2">
                  <input
                    v-model.number="occupationInputs[zone.id]"
                    type="number"
                    min="0"
                    :max="zone.nombrePlaces"
                    class="w-24 px-2 py-1 border rounded text-sm"
                  />
                  <span class="text-xs text-gray-400">places occupées</span>
                  <button class="ml-auto px-3 py-1 rounded border text-xs hover:bg-gray-50" @click="ajusterOccupation(zone)">
                    Mettre à jour
                  </button>
                </div>
              </template>
            </div>

            <p v-if="parking.zones.length === 0" class="text-sm text-gray-400">Aucune zone pour l'instant.</p>
          </div>

          <!-- Ajout d'une nouvelle zone -->
          <div class="border-t pt-4">
            <h4 class="font-medium mb-2">Ajouter une zone</h4>
            <div class="flex gap-2">
              <input v-model="newZone.nom" placeholder="Nom (ex: Zone C)" class="flex-1 px-3 py-2 border rounded-lg" />
              <select v-model="newZone.type" class="w-32 px-3 py-2 border rounded-lg text-sm">
                <option v-for="t in typesZone" :key="t" :value="t">{{ t }}</option>
              </select>
              <input v-model.number="newZone.nombrePlaces" type="number" min="1" placeholder="Places" class="w-28 px-3 py-2 border rounded-lg" />
              <button class="px-4 py-2 rounded-lg bg-primary text-white" @click="ajouterZone">Ajouter</button>
            </div>
            <p v-if="newZoneError" class="text-danger text-xs mt-1">{{ newZoneError }}</p>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>
