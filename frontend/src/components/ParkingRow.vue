<script setup>
import { computed } from 'vue'
import { authStore } from '../store/auth'
import { PERMISSIONS } from '../services/permissions'

const props = defineProps({
  parking: { type: Object, required: true }
})
defineEmits(['voir', 'modifier', 'supprimer'])

const barColor = computed(() => {
  const taux = props.parking.tauxOccupation
  if (taux > 90) return 'bg-danger'
  if (taux >= 70) return 'bg-warning'
  return 'bg-success'
})

const statutBadge = computed(() =>
  props.parking.statut === 'Actif'
    ? 'bg-success/10 text-success'
    : 'bg-gray-100 text-gray-500'
)
</script>

<template>
  <tr class="border-b last:border-0 hover:bg-gray-50">
    <td class="p-3">
      <div class="font-semibold">{{ parking.nom }}</div>
      <div class="text-xs text-gray-400">{{ parking.adresse }}</div>
    </td>
    <td class="p-3">
      <span class="font-mono text-xs bg-gray-100 px-2 py-1 rounded">{{ parking.code }}</span>
    </td>
    <td class="p-3">
      <span class="inline-flex px-2 py-1 rounded-full text-xs font-medium" :class="statutBadge">
        {{ parking.statut }}
      </span>
    </td>
    <td class="p-3 text-gray-600">{{ parking.nombreZones }}</td>
    <td class="p-3 w-56">
      <div class="flex items-center gap-2">
        <div class="flex-1 h-2 bg-gray-100 rounded-full overflow-hidden">
          <div
            class="h-full rounded-full transition-all"
            :class="barColor"
            :style="{ width: Math.min(parking.tauxOccupation, 100) + '%' }"
          />
        </div>
        <span class="text-xs text-gray-500 whitespace-nowrap">
          {{ parking.placesOccupees }}/{{ parking.capaciteMax }}
        </span>
      </div>
    </td>
    <td class="p-3">
      <div class="flex gap-2">
        <button class="px-3 py-1.5 rounded-lg border text-xs font-medium hover:bg-gray-50" @click="$emit('voir')">
          Détail
        </button>
        <template v-if="authStore.can(PERMISSIONS.PARKINGS_WRITE)">
          <button class="px-3 py-1.5 rounded-lg border text-xs font-medium hover:bg-gray-50" @click="$emit('modifier')">
            Modifier
          </button>
          <button
            class="px-3 py-1.5 rounded-lg border border-danger/30 text-danger text-xs font-medium hover:bg-danger/5"
            @click="$emit('supprimer')"
          >
            Archiver
          </button>
        </template>
      </div>
    </td>
  </tr>
</template>
