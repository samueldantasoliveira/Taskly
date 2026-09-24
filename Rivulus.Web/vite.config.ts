import react from '@vitejs/plugin-react'
import { defineConfig, loadEnv } from 'vite'

export default defineConfig(({ mode }) => {
  const environment = loadEnv(mode, process.cwd(), '')

  if (mode === 'production') {
    validateProductionApiUrl(environment.VITE_API_URL)
  }

  return {
    plugins: [react()],
    build: {
      sourcemap: false,
    },
  }
})

function validateProductionApiUrl(value: string | undefined) {
  if (!value) {
    throw new Error('VITE_API_URL is required for a production build.')
  }

  let url: URL

  try {
    url = new URL(value)
  } catch {
    throw new Error('VITE_API_URL must be a valid absolute URL.')
  }

  if (
    url.protocol !== 'https:'
    || value.endsWith('/')
    || url.username
    || url.password
    || url.pathname !== '/'
    || url.search
    || url.hash
  ) {
    throw new Error(
      'VITE_API_URL must be an HTTPS origin without credentials, path, query, hash, or trailing slash.',
    )
  }
}
