import { defineConfig, devices } from '@playwright/test'

// Runs against the shop started by Aspire (`dotnet run --project Kofi.AppHost`).
export default defineConfig({
  testDir: './e2e',
  timeout: 60_000,
  use: {
    baseURL: process.env.KOFI_WEB_URL ?? 'http://localhost:5173',
    trace: 'retain-on-failure',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
})
