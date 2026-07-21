<script setup>
import { ref } from 'vue'
import { authStore } from '../store/auth'

const email = ref('')
const password = ref('')
const errorMessage = ref('')
const isLoading = ref(false)

async function handleLogin() {
  errorMessage.value = ''
  isLoading.value = true
  try {
    await authStore.login(email.value, password.value)
  } catch (error) {
    errorMessage.value = 'Email ou mot de passe incorrect.'
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
    <div class="min-h-screen flex items-center justify-center bg-bg">
        <form @submit.prevent="handleLogin" class="bg-panel shadow-soft rounded-2xl p-10 w-96">
            <h1 class="text-2xl font-bold text-primary text-center">ParkLine</h1>  <p class="text-center text-slate-500 mb-6">Connexion administrateur</p>

            <label class="block text-sm font-medium text-slate-700 mb-1">Email</label>
            <input v-model="email"
                   type="email"
                   required
                   class="w-full border border-slate-300 rounded-lg px-3 py-2 mb-4 focus:outline-none focus:ring-2 focus:ring-primary" />

            <label class="block text-sm font-medium text-slate-700 mb-1">Mot de passe</label>
            <input v-model="password"
                   type="password"
                   required
                   class="w-full border border-slate-300 rounded-lg px-3 py-2 mb-4 focus:outline-none focus:ring-2 focus:ring-primary" />

            <p v-if="errorMessage" class="text-danger text-sm mb-4">{{ errorMessage }}</p>

            <button type="submit"
                    :disabled="isLoading"
                    class="w-full bg-primary hover:bg-accent text-white font-semibold py-2 rounded-lg transition disabled:opacity-50">
                {{ isLoading ? 'Connexion...' : 'Se connecter' }}
            </button>
        </form>
    </div>
</template>