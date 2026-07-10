<script setup>
defineProps({
  titre: { type: String, default: 'Ce module' }
})
</script>

<template>
  <div class="p-6 flex items-center justify-center h-full">
    <div class="text-center max-w-sm">
      <div class="w-14 h-14 mx-auto mb-4 rounded-full bg-primary/10 text-primary flex items-center justify-center">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" class="w-6 h-6">
          <circle cx="12" cy="12" r="10" />
          <path d="M12 8v4l3 3" />
        </svg>
      </div>
      <h2 class="text-lg font-semibold text-gray-700 mb-1">{{ titre }}</h2>
      <p class="text-sm text-gray-400">Ce module est en cours de développement et sera bientôt disponible.</p>
    </div>
  </div>
</template>
