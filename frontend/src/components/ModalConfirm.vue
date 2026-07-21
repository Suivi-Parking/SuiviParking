<template>
  <div style="position:fixed;inset:0;background:rgba(0,0,0,0.4);display:flex;align-items:center;justify-content:center;z-index:1000;">
    <div class="bg-panel" style="width:420px;border-radius:12px;padding:28px;box-shadow:0 8px 32px rgba(0,0,0,0.18);">

      <div style="display:flex;align-items:center;gap:12px;margin-bottom:16px;">
        <div :style="`width:40px;height:40px;border-radius:50%;background:${iconBg};display:flex;align-items:center;justify-content:center;font-size:20px;`">
          {{ icon }}
        </div>
        <h3 class="font-medium" style="font-size:16px;">{{ title }}</h3>
      </div>

      <p style="color:#6b7280;font-size:14px;margin-bottom:24px;">{{ message }}</p>

      <div style="display:flex;justify-content:flex-end;gap:10px;">
        <button v-if="showCancel" type="button" @click="$emit('cancel')">
          {{ cancelLabel }}
        </button>
        <button type="button" :style="`background:${confirmBg};color:white;border:none;padding:8px 16px;border-radius:6px;cursor:pointer;`" @click="$emit('confirm')">
          {{ confirmLabel }}
        </button>
      </div>

    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  title: { type: String, default: 'Confirmation' },
  message: { type: String, default: '' },
  confirmLabel: { type: String, default: 'Confirmer' },
  cancelLabel: { type: String, default: 'Annuler' },
  showCancel: { type: Boolean, default: true },
  type: { type: String, default: 'danger' }
})

defineEmits(['confirm', 'cancel'])

const icon = computed(() => {
  const icons = { danger: '🗑️', success: '✅', info: 'ℹ️', warning: '⚠️' }
  return icons[props.type] ?? 'ℹ️'
})

const iconBg = computed(() => {
  const bgs = { danger: '#fee2e2', success: '#dcfce7', info: '#dbeafe', warning: '#fef9c3' }
  return bgs[props.type] ?? '#dbeafe'
})

const confirmBg = computed(() => {
  const bgs = { danger: '#ef4444', success: '#22c55e', info: '#3b82f6', warning: '#f59e0b' }
  return bgs[props.type] ?? '#3b82f6'
})
</script>