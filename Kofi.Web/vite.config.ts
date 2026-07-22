import path from 'node:path'
import tailwindcss from '@tailwindcss/vite'
import { tanstackRouter } from '@tanstack/router-plugin/vite'
import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Aspire injects the API address; the fallback is a plain `dotnet run` of Kofi.API.
const apiUrl =
  process.env.API_HTTP ?? process.env.services__api__http__0 ?? 'http://localhost:5276'

export default defineConfig({
  plugins: [
    tanstackRouter({ target: 'react', autoCodeSplitting: true }),
    react(),
    tailwindcss(),
  ],
  resolve: {
    alias: { '@': path.resolve(__dirname, './src') },
  },
  server: {
    port: Number(process.env.PORT) || 5173,
    proxy: {
      // Same origin for the browser, so no CORS. The Host header is kept, so the
      // API builds the payment return URL on this origin and the provider sends
      // the customer back into the SPA.
      '/api': {
        target: apiUrl,
        changeOrigin: false,
        rewrite: (p) => p.replace(/^\/api/, ''),
      },
    },
  },
})
