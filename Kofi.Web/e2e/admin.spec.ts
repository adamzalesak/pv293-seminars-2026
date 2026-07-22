import { expect, test } from '@playwright/test'
import { signIn } from './helpers'

test('an administrator adds a product and changes its price', async ({ page, request }) => {
  const name = `Test Kenya AA ${Date.now()}`
  await signIn(page, 'admin@kofi.test', 'Admin123!')
  try {
    await page.goto('/admin/products')
    await page.getByRole('button', { name: 'New product' }).click()
    await page.getByLabel('Name').fill(name)
    await page.getByLabel('Price (EUR)').fill('14.20')
    await page.getByLabel('Stock').fill('30')
    await page.getByLabel('Origin').fill('Keňa')
    await page.getByRole('button', { name: 'Add product' }).click()
    await expect(page.getByRole('cell', { name, exact: true })).toBeVisible()

    await page.getByRole('button', { name: `Actions for ${name}` }).click()
    await page.getByRole('menuitem', { name: 'Change price' }).click()
    await page.getByLabel('New price (EUR)').fill('15.80')
    await page.getByRole('button', { name: 'Change price' }).click()
    await expect(page.getByRole('row', { name: new RegExp(name) })).toContainText('€15.80')

    await page.goto(`/?q=${encodeURIComponent(name)}`)
    await expect(page.getByRole('heading', { name })).toBeVisible()
    await expect(page.getByText('€15.80')).toBeVisible()
  } finally {
    // Leave the catalogue as the seed made it, whatever happened above.
    const login = await request.post('/api/auth/login', { data: { email: 'admin@kofi.test', password: 'Admin123!' } })
    const { accessToken } = await login.json()
    const found = await (await request.get(`/api/products?search=${encodeURIComponent(name)}`)).json()
    for (const product of found.items) {
      await request.post(`/api/products/${product.id}/deactivate`, { headers: { Authorization: `Bearer ${accessToken}` } })
    }
  }
})

test('a customer cannot open the admin', async ({ page }) => {
  await signIn(page, 'petra@example.com', 'Customer123!')
  await page.goto('/admin')
  await expect(page.getByText('No access')).toBeVisible()
})

test('signing in is required for orders', async ({ page }) => {
  await page.goto('/orders')
  await expect(page).toHaveURL(/\/login\?redirect=/)
})

test('a token the API rejects sends the person back to sign in', async ({ page }) => {
  // Well-formed and not expired, but not signed by the shop.
  const payload = btoa(JSON.stringify({ email: 'petra@example.com', role: 'Customer', exp: Date.now() / 1000 + 3600 }))
  await page.addInitScript((token) => localStorage.setItem('kofi.token', token), `x.${payload}.x`)
  await page.goto('/orders')
  await expect(page).toHaveURL(/\/login\?redirect=/)
  await expect(page.getByText('Your session has ended')).toBeVisible()
})
