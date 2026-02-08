import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:7032',
        changeOrigin: true,
        secure: false,
      },
      '/payments': {
        target: 'http://localhost:7032',
        changeOrigin: true,
        secure: false,
      },
      '/notifications': {
        target: 'http://localhost:7032',
        changeOrigin: true,
        secure: false,
      },
      '/auth': {
        target: 'http://localhost:7032',
        changeOrigin: true,
        secure: false,
      },
      '/users': {
        target: 'http://localhost:7032',
        changeOrigin: true,
        secure: false,
      }
    }
  },
  resolve: {
    alias: {
      "@": "/src",
    },
  },
})
