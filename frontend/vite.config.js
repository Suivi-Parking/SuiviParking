import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

export default defineConfig({
    plugins: [vue()],
    server: {
        port: 5174,
        open: false,
        proxy: {
            '/api': {
                target: 'http://localhost:5104',
                changeOrigin: true,
            }
        }
    }
})