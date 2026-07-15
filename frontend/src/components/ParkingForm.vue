<script setup>
import { reactive, ref, watch } from 'vue'

const props = defineProps({
  parking: { type: Object, default: null } // null = creation, objet = edition (infos generales seulement)
})
const emit = defineEmits(['save', 'cancel'])

const isEdition = !!props.parking

const form = reactive({
  nom: '',
  code: '',
  adresse: '',
  latitude: null,
  longitude: null,
  ouvert24h24: false,
  heureOuverture: '',
  heureFermeture: '',
  imageUrl: '',
  statut: 'Actif'
})

// Une zone a au moins un id local (pour v-for :key), pas encore d'id serveur
let prochainIdLocal = 1
const zones = ref([{ idLocal: prochainIdLocal++, nom: '', type: 'Standard', nombrePlaces: null }])

const typesZone = ['Standard', 'Visiteurs', 'Residents', 'VIP', 'Livraison', 'Autre']

const errors = reactive({})

watch(
  () => props.parking,
  (p) => {
    if (p) {
      form.nom = p.nom
      form.code = p.code
      form.adresse = p.adresse
      form.latitude = p.latitude
      form.longitude = p.longitude
      form.ouvert24h24 = p.ouvert24h24
      form.heureOuverture = p.heureOuverture?.slice(0, 5) ?? ''
      form.heureFermeture = p.heureFermeture?.slice(0, 5) ?? ''
      form.imageUrl = p.imageUrl ?? ''
      form.statut = p.statut
    }
  },
  { immediate: true }
)

function ajouterZone() {
  zones.value.push({ idLocal: prochainIdLocal++, nom: '', type: 'Standard', nombrePlaces: null })
}
function supprimerZone(idLocal) {
  if (zones.value.length <= 1) return // toujours garder au moins 1 zone dans le formulaire
  zones.value = zones.value.filter(z => z.idLocal !== idLocal)
}

// Un input time HTML donne "HH:MM" ; le backend (TimeOnly) veut "HH:MM:SS"
function normaliserHeure(valeur) {
  if (!valeur) return null
  return valeur.length === 5 ? `${valeur}:00` : valeur
}

function validate() {
  errors.nom = !form.nom ? 'Le nom est requis' : ''
  errors.code = !form.code ? 'Le code est requis' : ''
  errors.adresse = !form.adresse ? "L'adresse est requise" : ''

  if (!form.ouvert24h24) {
    errors.horaires = (!form.heureOuverture || !form.heureFermeture)
      ? "Horaires requis (ou cochez 'Ouvert 24h/24')"
      : ''
  } else {
    errors.horaires = ''
  }

  if (!isEdition) {
    const zonesInvalides = zones.value.some(z => !z.nom || !z.nombrePlaces || z.nombrePlaces <= 0)
    errors.zones = zonesInvalides ? 'Chaque zone doit avoir un nom et un nombre de places > 0' : ''
  }

  return !Object.values(errors).some(Boolean)
}

function submit() {
  if (!validate()) return

  const payload = {
    nom: form.nom,
    code: form.code,
    adresse: form.adresse,
    latitude: form.latitude,
    longitude: form.longitude,
    ouvert24h24: form.ouvert24h24,
    heureOuverture: form.ouvert24h24 ? null : normaliserHeure(form.heureOuverture),
    heureFermeture: form.ouvert24h24 ? null : normaliserHeure(form.heureFermeture),
    imageUrl: form.imageUrl || null,
    statut: form.statut
  }

  // Les zones ne sont envoyees qu'a la creation : la modification des zones
  // se fait ensuite via la vue detail (ParkingDetailModal), pas ici.
  if (!isEdition) {
    payload.zones = zones.value.map(z => ({
      nom: z.nom,
      type: z.type,
      nombrePlaces: z.nombrePlaces
    }))
  }

  emit('save', payload)
}
</script>

<template>
  <div class="fixed inset-0 bg-gray-900/40 flex items-center justify-center z-50 p-4">
    <div class="bg-white rounded-xl shadow-xl w-[600px] max-h-[90vh] overflow-y-auto">
      <header class="p-4 border-b flex justify-between items-center sticky top-0 bg-white">
        <h3 class="font-semibold text-lg">{{ isEdition ? 'Modifier' : 'Nouveau' }} parking</h3>
        <button type="button" class="text-gray-400 hover:text-gray-600" @click="$emit('cancel')">✕</button>
      </header>

      <form @submit.prevent="submit" class="p-4 space-y-4">
        <!-- Informations generales -->
        <div class="grid grid-cols-2 gap-3">
          <div class="col-span-2">
            <label class="block text-sm mb-1">Nom *</label>
            <input v-model="form.nom" class="w-full px-3 py-2 border rounded-lg" />
            <p v-if="errors.nom" class="text-danger text-xs mt-1">{{ errors.nom }}</p>
          </div>

          <div>
            <label class="block text-sm mb-1">Code unique *</label>
            <input v-model="form.code" placeholder="ex: PARK-CENTRE" class="w-full px-3 py-2 border rounded-lg" />
            <p v-if="errors.code" class="text-danger text-xs mt-1">{{ errors.code }}</p>
          </div>

          <div>
            <label class="block text-sm mb-1">Statut</label>
            <select v-model="form.statut" class="w-full px-3 py-2 border rounded-lg">
              <option value="Actif">Actif</option>
              <option value="Inactif">Inactif</option>
            </select>
          </div>

          <div class="col-span-2">
            <label class="block text-sm mb-1">Adresse *</label>
            <input v-model="form.adresse" class="w-full px-3 py-2 border rounded-lg" />
            <p v-if="errors.adresse" class="text-danger text-xs mt-1">{{ errors.adresse }}</p>
          </div>

          <div>
            <label class="block text-sm mb-1">Latitude</label>
            <input v-model.number="form.latitude" type="number" step="0.000001" class="w-full px-3 py-2 border rounded-lg" />
          </div>
          <div>
            <label class="block text-sm mb-1">Longitude</label>
            <input v-model.number="form.longitude" type="number" step="0.000001" class="w-full px-3 py-2 border rounded-lg" />
          </div>

          <div class="col-span-2">
            <label class="block text-sm mb-1">Image (URL)</label>
            <input v-model="form.imageUrl" placeholder="https://..." class="w-full px-3 py-2 border rounded-lg" />
          </div>
        </div>

        <!-- Horaires -->
        <div class="border-t pt-3">
          <label class="flex items-center gap-2 text-sm mb-2">
            <input type="checkbox" v-model="form.ouvert24h24" />
            Ouvert 24h/24
          </label>
          <div v-if="!form.ouvert24h24" class="grid grid-cols-2 gap-3">
            <div>
              <label class="block text-sm mb-1">Heure d'ouverture</label>
              <input v-model="form.heureOuverture" type="time" class="w-full px-3 py-2 border rounded-lg" />
            </div>
            <div>
              <label class="block text-sm mb-1">Heure de fermeture</label>
              <input v-model="form.heureFermeture" type="time" class="w-full px-3 py-2 border rounded-lg" />
            </div>
          </div>
          <p v-if="errors.horaires" class="text-danger text-xs mt-1">{{ errors.horaires }}</p>
        </div>

        <!-- Zones (creation uniquement - au moins 1 obligatoire) -->
        <div v-if="!isEdition" class="border-t pt-3">
          <div class="flex items-center justify-between mb-2">
            <h4 class="font-medium text-sm">Zones (au moins 1 requise)</h4>
            <button type="button" class="text-xs text-primary" @click="ajouterZone">+ Ajouter une zone</button>
          </div>

          <div v-for="zone in zones" :key="zone.idLocal" class="flex gap-2 mb-2 items-start">
            <input v-model="zone.nom" placeholder="Nom (ex: Zone A)" class="flex-1 px-2 py-1.5 border rounded text-sm" />
            <select v-model="zone.type" class="w-32 px-2 py-1.5 border rounded text-sm">
              <option v-for="t in typesZone" :key="t" :value="t">{{ t }}</option>
            </select>
            <input v-model.number="zone.nombrePlaces" type="number" min="1" placeholder="Places" class="w-24 px-2 py-1.5 border rounded text-sm" />
            <button
              type="button"
              class="text-gray-400 hover:text-danger px-1"
              :disabled="zones.length <= 1"
              @click="supprimerZone(zone.idLocal)"
            >✕</button>
          </div>
          <p v-if="errors.zones" class="text-danger text-xs mt-1">{{ errors.zones }}</p>
          <p class="text-xs text-gray-400 mt-1">
            La capacité totale sera calculée automatiquement (somme des zones).
          </p>
        </div>
        <p v-else class="text-xs text-gray-400 border-t pt-3">
          Les zones se gèrent depuis la vue "Détail" du parking (ajout, modification, activation/désactivation).
        </p>

        <div class="flex justify-end gap-2 pt-2">
          <button type="button" class="px-4 py-2 rounded-lg border" @click="$emit('cancel')">Annuler</button>
          <button type="submit" class="px-4 py-2 rounded-lg bg-primary text-white">
            {{ isEdition ? 'Enregistrer' : 'Créer' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
