import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
    plugins: [vue()],
    server: {
        port: 5173,
        open: false,
        proxy: {
            '/api': {
                target: 'http://localhost:5226',
                changeOrigin: true,
            }
        }
    }
})
