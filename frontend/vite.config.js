import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [vue()],
    server: {
        port: 5173,
        open: false,
        proxy: {
            '/api': {
                // HTTPS directement (pas HTTP) : evite le 307 UseHttpsRedirection()
                // du backend, qui faisait perdre le header Authorization lors de la
                // redirection cross-port (HTTP:5226 -> HTTPS:7080).
                target: 'https://localhost:7080',
                changeOrigin: true,
                secure: false, // accepte le certificat auto-signe du backend en dev
            }
        }
    }
})