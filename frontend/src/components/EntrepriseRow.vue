<template>
  <tr class="bg-panel border-b">
    <td class="p-4 flex items-center gap-4">
      <div class="w-10 h-10 rounded-md bg-slate-100 flex items-center justify-center font-semibold text-sm text-slate-700">
        {{ initials }}
      </div>
      <div>
        <div class="font-medium">{{ entreprise.nom }}</div>
        <div class="text-xs text-muted">{{ entreprise.matriculeFiscal }}</div>
      </div>
    </td>
    <td class="p-4"><span class="text-sm bg-[var(--bg)] px-3 py-1 rounded-full text-muted">{{ entreprise.secteur }}</span></td>
    <td class="p-4 text-right">{{ entreprise.nombreEmployes }}</td>
    <td class="p-4">
      <span class="inline-flex items-center px-3 py-1 rounded-full text-sm" :class="statusBg">
        <span class="w-2 h-2 rounded-full mr-2" :style="{background: statusDot}"></span>{{ statutText }}
      </span>
    </td>
    <td class="p-4">{{ formattedDate }}</td>
    <td class="p-4 text-center space-x-2 text-muted">
      <button type="button" title="Voir" @click="emit('view', entreprise)">👁️</button>
      <template v-if="authStore.can(PERMISSIONS.ENTREPRISES_WRITE)">
        <button type="button" title="Modifier" @click="emit('edit', entreprise)">✏️</button>
        <button type="button" title="Supprimer" @click="emit('delete', entreprise)">🗑️</button>
      </template>
    </td>
  </tr>
</template>

<script setup>
import { computed } from 'vue'
import { authStore } from '../store/auth'
import { PERMISSIONS } from '../services/permissions'
const props = defineProps({ entreprise: Object })
const emit = defineEmits(['view','edit','delete'])

const initials = computed(() => (props.entreprise.nom || '').split(' ').map(s=>s[0]).slice(0,2).join('').toUpperCase())
const statutText = computed(() => props.entreprise.statutAbonnement || 'EnAttente')
const statusDot = computed(() => statutText.value === 'Actif' ? 'var(--success)' : statutText.value === 'EnAttente' ? 'var(--warning)' : 'var(--danger)')
const statusBg = computed(() => statutText.value === 'Actif' ? 'bg-[rgba(46,204,113,0.08)] text-[var(--success)]' : statutText.value === 'EnAttente' ? 'bg-[rgba(245,158,11,0.08)] text-[var(--warning)]' : 'bg-[rgba(239,68,68,0.06)] text-[var(--danger)]')
const formattedDate = computed(()=> new Date(props.entreprise.dateCreation || Date.now()).toLocaleDateString())
</script>
