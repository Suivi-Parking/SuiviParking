<script setup>
import { reactive, watch } from 'vue'

const props = defineProps({
  parking: { type: Object, default: null } // null = creation, objet = edition
})
const emit = defineEmits(['save', 'cancel'])

const form = reactive({
  nom: '',
  adresse: '',
  capaciteMax: 0
})

const errors = reactive({})

watch(
  () => props.parking,
  (p) => {
    if (p) {
      form.nom = p.nom
      form.adresse = p.adresse
      form.capaciteMax = p.capaciteMax
    }
  },
  { immediate: true }
)

function validate() {
  errors.nom = !form.nom ? 'Le nom est requis' : ''
  errors.adresse = !form.adresse ? "L'adresse est requise" : ''
  errors.capaciteMax = form.capaciteMax < 0 ? 'Capacité invalide' : ''
  return !Object.values(errors).some(Boolean)
}

function submit() {
  if (!validate()) return
  emit('save', { ...form })
}
</script>

<template>
  <div class="fixed inset-0 bg-gray-900/40 flex items-center justify-center z-50">
    <div class="bg-white rounded-xl shadow-xl w-[480px] overflow-hidden">
      <header class="p-4 border-b flex justify-between items-center">
        <h3 class="font-semibold text-lg">{{ parking ? 'Modifier' : 'Nouveau' }} parking</h3>
        <button type="button" class="text-gray-400 hover:text-gray-600" @click="$emit('cancel')">✕</button>
      </header>

      <form @submit.prevent="submit" class="p-4 space-y-3">
        <div>
          <label class="block text-sm mb-1">Nom *</label>
          <input v-model="form.nom" class="w-full px-3 py-2 border rounded-lg" />
          <p v-if="errors.nom" class="text-danger text-xs mt-1">{{ errors.nom }}</p>
        </div>

        <div>
          <label class="block text-sm mb-1">Adresse *</label>
          <input v-model="form.adresse" class="w-full px-3 py-2 border rounded-lg" />
          <p v-if="errors.adresse" class="text-danger text-xs mt-1">{{ errors.adresse }}</p>
        </div>

        <div>
          <label class="block text-sm mb-1">Capacité maximale *</label>
          <input v-model.number="form.capaciteMax" type="number" min="0" class="w-full px-3 py-2 border rounded-lg" />
          <p v-if="errors.capaciteMax" class="text-danger text-xs mt-1">{{ errors.capaciteMax }}</p>
          <p class="text-xs text-gray-400 mt-1">Indicative — la capacité réelle se calcule à partir des zones.</p>
        </div>

        <div class="flex justify-end gap-2 pt-2">
          <button type="button" class="px-4 py-2 rounded-lg border" @click="$emit('cancel')">Annuler</button>
          <button type="submit" class="px-4 py-2 rounded-lg bg-primary text-white">
            {{ parking ? 'Enregistrer' : 'Créer' }}
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
