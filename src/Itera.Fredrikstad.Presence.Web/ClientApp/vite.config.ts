import { defineConfig } from 'vite'
import { svelte } from '@sveltejs/vite-plugin-svelte'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [svelte()],
  server: {
    hmr: {
      "protocol": "ws"
    },
    proxy: {
      "^/api": {
        target: "http://localhost:5277",
        changeOrigin: true,
        secure: false
      }
    }
  }
})
