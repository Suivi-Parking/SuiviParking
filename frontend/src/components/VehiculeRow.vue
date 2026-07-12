<template>
  <tr class="bg-panel hover:bg-gray-50">
    <td class="p-3">
      <div style="display:flex;align-items:center;gap:10px;">
        <div class="avatar" style="background:#dbeafe;">
          🚗
        </div>
        <div>
          <div class="font-medium">{{ vehicule.immatriculation }}</div>
          <div class="text-muted" style="font-size:12px;">ID-{{ vehicule.id }}</div>
        </div>
      </div>
    </td>
    <td class="p-3">
      <div class="font-medium">{{ vehicule.marque }}</div>
      <div class="text-muted" style="font-size:12px;">{{ vehicule.modele }}</div>
    </td>
    <td class="p-3 text-muted">{{ vehicule.couleur }}</td>
    <td class="p-3 text-muted">{{ vehicule.type }}</td>
    <td class="p-3 text-muted">{{ vehicule.employeNom ?? 'N/A' }}</td>
    <td class="p-3">
      <span :class="statutClass">● {{ vehicule.statut }}</span>
    </td>
    <td class="p-3">
      <div style="display:flex;gap:6px;">
        <button type="button" @click="$emit('view', vehicule)" title="Voir">👁️</button>
        <button type="button" @click="$emit('edit', vehicule)" title="Modifier">✏️</button>
        <button type="button" @click="$emit('delete', vehicule)"
          title="Supprimer" style="color:#ef4444;">🗑️</button>
      </div>
    </td>
  </tr>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  vehicule: { type: Object, required: true }
})

defineEmits(['view', 'edit', 'delete'])

const statutClass = computed(() => {
  return props.vehicule.statut === 'ACTIF' ? 'stat-actif' : 'stat-expire'
})
</script>