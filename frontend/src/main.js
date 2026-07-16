import { createApp } from 'vue'
import App from './App.vue'
import './assets/styles.css'
import './assets/fallback.css'

// Les endpoints backend exigent désormais un JWT (Authorize + HasPermission).
// Certains composants appellent fetch('/api/...') directement (sans passer par
// services/api.js), on intercepte donc fetch() globalement pour y ajouter
// automatiquement l'en-tête Authorization quand un token est présent.
const nativeFetch = window.fetch.bind(window)
window.fetch = (input, init = {}) => {
    const url = typeof input === 'string' ? input : input?.url ?? ''
    const token = localStorage.getItem('token')

    if (token && url.startsWith('/api')) {
        const headers = new Headers(init.headers || (typeof input !== 'string' ? input.headers : undefined))
        if (!headers.has('Authorization')) {
            headers.set('Authorization', `Bearer ${token}`)
        }
        init = { ...init, headers }
    }


createApp(App).mount('#app')
