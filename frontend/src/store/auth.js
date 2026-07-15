import { reactive } from 'vue'
import api from '../services/api'

export const authStore = reactive({
    token: localStorage.getItem('token') || '',
    email: localStorage.getItem('email') || '',
    role: localStorage.getItem('role') || '',

    get isAuthenticated() {
        return !!this.token
    },

    async login(email, motDePasse) {
        const response = await api.post('/Auth/login', { email, motDePasse })
        this.token = response.data.token
        this.email = response.data.email
        this.role = response.data.role
        localStorage.setItem('token', this.token)
        localStorage.setItem('email', this.email)
        localStorage.setItem('role', this.role)
    },

    logout() {
        this.token = ''
        this.email = ''
        this.role = ''
        localStorage.removeItem('token')
        localStorage.removeItem('email')
        localStorage.removeItem('role')
    },
})