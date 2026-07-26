import { reactive } from 'vue'
import api from '../services/api'

function readStoredPermissions() {
    try {
        return JSON.parse(localStorage.getItem('permissions') || '[]')
    } catch {
        return []
    }
}

export const authStore = reactive({
    token: localStorage.getItem('token') || '',
    email: localStorage.getItem('email') || '',
    role: localStorage.getItem('role') || '',
    permissions: readStoredPermissions(),

    get isAuthenticated() {
        return !!this.token
    },

    can(permission) {
        return this.permissions.includes(permission)
    },

    async login(email, motDePasse) {
        const response = await api.post('/Auth/login', { email, motDePasse })
        this.token = response.data.token
        this.email = response.data.email
        this.role = response.data.role
        this.permissions = response.data.permissions || []
        localStorage.setItem('token', this.token)
        localStorage.setItem('email', this.email)
        localStorage.setItem('role', this.role)

        // Le login ne renvoie PAS les permissions (voir LoginResponse cote backend) :
        // il faut les recuperer separement via GET /Auth/permissions, une fois le
        // token deja stocke ci-dessus (l'intercepteur axios en a besoin pour l'envoyer).
        const permsResponse = await api.get('/Auth/permissions')
        this.permissions = permsResponse.data.permissions || []
        localStorage.setItem('permissions', JSON.stringify(this.permissions))
    },

    logout() {
        this.token = ''
        this.email = ''
        this.role = ''
        this.permissions = []
        localStorage.removeItem('token')
        localStorage.removeItem('email')
        localStorage.removeItem('role')
        localStorage.removeItem('permissions')
    },
})
