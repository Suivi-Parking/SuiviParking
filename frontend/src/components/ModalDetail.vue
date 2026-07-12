<template>
  <div style="position:fixed;inset:0;background:rgba(0,0,0,0.4);display:flex;align-items:center;justify-content:center;z-index:1000;">
    <div class="bg-panel" style="width:480px;border-radius:12px;padding:28px;box-shadow:0 8px 32px rgba(0,0,0,0.18);">
      
      <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:20px;">
        <h3 class="font-medium" style="font-size:16px;">{{ title }}</h3>
        <button type="button" @click="$emit('close')" 
          style="background:none;border:none;font-size:20px;cursor:pointer;color:#6b7280;">✕</button>
      </div>

      <div style="display:grid;gap:12px;">
        <div v-for="field in fields" :key="field.key"
          style="display:flex;justify-content:space-between;padding:10px;background:#f8fafc;border-radius:8px;">
          <span class="text-muted" style="font-size:13px;">{{ field.label }}</span>
          <span style="font-size:13px;font-weight:600;">{{ field.value }}</span>
        </div>
      </div>

      <div style="display:flex;justify-content:flex-end;margin-top:24px;">
        <button type="button" class="primary" @click="$emit('close')">Fermer</button>
      </div>

    </div>
  </div>
</template>

<script setup>
defineProps({
  title: { type: String, default: 'Détails' },
  fields: { type: Array, default: () => [] }
})
defineEmits(['close'])
</script>