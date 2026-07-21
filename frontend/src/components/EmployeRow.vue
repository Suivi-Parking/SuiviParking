<template>
  <tr class="bg-panel hover:bg-gray-50">
    <td class="p-3">
      <div style="display:flex;align-items:center;gap:10px;">
        <div class="avatar" :style="{ background: avatarColor }">
          {{ initiales }}
        </div>
        <div>
          <div class="font-medium">{{ employe.nom }} {{ employe.prenom }}</div>
          <div class="text-muted" style="font-size:12px;">ID-{{ employe.id }}</div>
        </div>
      </div>
    </td>
    <td class="p-3 text-muted">{{ employe.email }}</td>
    <td class="p-3 text-muted">{{ employe.telephone }}</td>
    <td class="p-3">{{ employe.fonction }}</td>
    <td class="p-3">
      <span :class="statutClass">● {{ employe.statut }}</span>
    </td>
    <td class="p-3 text-muted">
      {{ employe.vehicules?.length ?? 0 }} véhicule(s)
    </td>
    <td class="p-3">
      <div style="display:flex;gap:6px;">
        <button type="button" @click="$emit('view', employe)" title="Voir">👁️</button>
        <button type="button" @click="$emit('edit', employe)" title="Modifier">✏️</button>
        <button type="button" @click="$emit('delete', employe)" 
          title="Supprimer" style="color:#ef4444;">🗑️</button>
      </div>
    </td>
  </tr>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  employe: { type: Object, required: true }
})

defineEmits(['view', 'edit', 'delete'])

const initiales = computed(() => {
  const n = props.employe.nom?.[0] ?? ''
  const p = props.employe.prenom?.[0] ?? ''
  return (n + p).toUpperCase()
})

const avatarColor = computed(() => {
  const colors = ['#dbeafe', '#dcfce7', '#fef9c3', '#fce7f3', '#ede9fe']
  return colors[props.employe.id % colors.length]
})

const statutClass = computed(() => {
  return props.employe.statut === 'ACTIF' ? 'stat-actif' : 'stat-expire'
})
</script>