import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import path from 'path'

export default defineConfig(({ mode }) => {
  const isGitHubPages = mode === 'gh-pages';
  return {
    base: isGitHubPages ? '/Bank-Management-System-React-.NET-SQL/' : './',
    plugins: [react(), tailwindcss()],
    build: {
      outDir: isGitHubPages 
        ? path.resolve(import.meta.dirname, 'dist')
        : path.resolve(import.meta.dirname, '../EnterpriseBankingSystem.API/wwwroot'),
      emptyOutDir: true,
      rollupOptions: {
        output: {
          entryFileNames: 'assets/[name].js',
          chunkFileNames: 'assets/[name].js',
          assetFileNames: 'assets/[name].[ext]'
        }
      }
    }
  }
})
