<script setup>
import { computed } from 'vue'

const props = defineProps({
  parking: { type: Object, required: true }
})
defineEmits(['voir', 'modifier', 'supprimer'])

// Vert < 70%, ambre 70-90%, rouge > 90% - coherent avec les prompts Figma prepares
const barColor = computed(() => {
  const taux = props.parking.tauxOccupation
  if (taux > 90) return 'bg-danger'
  if (taux >= 70) return 'bg-warning'
  return 'bg-success'
})
</script>

<template>
  <tr class="border-b last:border-0 hover:bg-gray-50">
    <td class="p-3">
      <div class="font-semibold">{{ parking.nom }}</div>
      <div class="text-xs text-gray-400">{{ parking.adresse }}</div>
    </td>
    <td class="p-3 text-gray-600">{{ parking.capaciteMax }}</td>
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
          {{ parking.placesOccupees }}/{{ parking.placesTotales }}
        </span>
      </div>
    </td>
    <td class="p-3">
      <div class="flex gap-2">
        <button class="px-3 py-1.5 rounded-lg border text-xs font-medium hover:bg-gray-50" @click="$emit('voir')">
          Détail
        </button>
        <button class="px-3 py-1.5 rounded-lg border text-xs font-medium hover:bg-gray-50" @click="$emit('modifier')">
          Modifier
        </button>
        <button
          class="px-3 py-1.5 rounded-lg border border-danger/30 text-danger text-xs font-medium hover:bg-danger/5"
          @click="$emit('supprimer')"
        >
          Supprimer
        </button>
      </div>
    </td>
  </tr>
</template>
