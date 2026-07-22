import { defineConfig } from 'orval'

// openapi.json is a snapshot of the running API (`pnpm api:fetch`), so the client
// can be regenerated without starting the backend.
export default defineConfig({
  kofi: {
    input: { target: './openapi.json' },
    output: {
      mode: 'split',
      target: './src/api/generated/endpoints.ts',
      schemas: './src/api/generated/model',
      client: 'react-query',
      httpClient: 'fetch',
      clean: true,
      override: {
        mutator: { path: './src/api/fetcher.ts', name: 'apiFetch' },
        fetch: { includeHttpResponseReturnType: false },
        query: { version: 5, signal: true },
      },
    },
  },
})
