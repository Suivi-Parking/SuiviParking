<template>
  <div class="modal-backdrop" style="position:fixed;inset:0;display:flex;align-items:center;justify-content:center;z-index:60">
    <div class="modal" style="width:460px;background:white;border-radius:10px;box-shadow:0 10px 30px rgba(0,0,0,0.2);overflow:hidden">
      <header style="padding:16px;border-bottom:1px solid #eef2f6;display:flex;justify-content:space-between;align-items:center">
        <h3 style="margin:0">{{ title }}</h3>
        <button type="button" @click="$emit('cancel')" style="border:none;background:transparent;font-size:18px;cursor:pointer">✖</button>
      </header>

      <form @submit.prevent="submit" style="padding:16px;">
        <p class="text-sm text-muted" style="margin-top:0">
          {{ abonnement?.entrepriseNom }} — {{ abonnement?.typeAbonnement }}
        </p>

        <template v-if="mode === 'renouveler'">
          <label class="block text-sm">Nouvelle date de fin *</label>
          <input v-model="nouvelleDateFin" type="date" style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px" />
          <div v-if="error" class="text-sm" style="color:#dc2626">{{ error }}</div>
        </template>

        <template v-else>
          <label class="block text-sm">Raison *</label>
          <textarea v-model="raison" rows="3" style="width:100%;padding:8px;border:1px solid #cbd5e1;border-radius:6px"></textarea>
          <div v-if="error" class="text-sm" style="color:#dc2626">{{ error }}</div>
        </template>

        <div style="display:flex;justify-content:flex-end;gap:8px;margin-top:16px">
          <button type="button" @click="$emit('cancel')" style="padding:8px 12px;border-radius:6px;border:1px solid #cbd5e1;background:#fff">Annuler</button>
          <button type="submit" class="primary">{{ confirmLabel }}</button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  mode: { type: String, required: true }, // 'suspendre' | 'resilier' | 'renouveler'
  abonnement: { type: Object, default: null },
})
const emit = defineEmits(['confirm', 'cancel'])

const raison = ref('')
const nouvelleDateFin = ref('')
const error = ref('')

const titles = {
  suspendre: "Suspendre l'abonnement",
  resilier: "Résilier l'abonnement",
  renouveler: "Renouveler l'abonnement",
}
const title = computed(() => titles[props.mode] || 'Action')

const confirmLabels = {
  suspendre: 'Suspendre',
  resilier: 'Résilier',
  renouveler: 'Renouveler',
}
const confirmLabel = computed(() => confirmLabels[props.mode] || 'Confirmer')

function submit() {
  error.value = ''

  if (props.mode === 'renouveler') {
    if (!nouvelleDateFin.value) {
      error.value = 'La nouvelle date de fin est requise'
      return
    }
    emit('confirm', { NouvelleDateFin: nouvelleDateFin.value })
    return
  }

  if (!raison.value.trim()) {
    error.value = 'La raison est requise'
    return
  }
  emit('confirm', { Raison: raison.value.trim() })
}
</script>

<style scoped>
.modal-backdrop{background:rgba(15,23,42,0.4)}
</style>
