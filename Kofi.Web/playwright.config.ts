import { defineConfig, devices } from '@playwright/test'

// Runs against the shop started by Aspire (`dotnet run --project Kofi.AppHost`).
export default defineConfig({
  testDir: './e2e',
  // Tests share the seeded data; in parallel they reserve the same stock and
  // trip over each other's concurrency checks.
  workers: 1,
  timeout: 60_000,
  use: {
    baseURL: process.env.KOFI_WEB_URL ?? 'http://localhost:5173',
    trace: 'retain-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
})
