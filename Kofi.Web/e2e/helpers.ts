import { expect, type APIRequestContext, type Page } from '@playwright/test'

export async function signIn(page: Page, email: string, password: string) {
  await page.goto('/login')
  await page.getByLabel('E-mail').fill(email)
  await page.getByLabel('Password').fill(password)
  await page.getByRole('button', { name: 'Sign in' }).click()
  await expect(page).not.toHaveURL(/\/login/)
}

export async function authHeader(request: APIRequestContext, email: string, password: string) {
  const login = await request.post('/api/auth/login', { data: { email, password } })
  return { Authorization: `Bearer ${(await login.json()).accessToken}` }
}

// Petra subscribes with the first delivery today and authorises her card at
// the payment provider; activation then prepares the first delivery as a paid order.
export async function activeSubscription(request: APIRequestContext) {
  const petra = await authHeader(request, 'petra@example.com', 'Customer123!')
  const kolumbie = (await (await request.get('/api/products?search=Kolumbie')).json()).items[0]
  const created = await request.post('/api/subscriptions', {
    headers: petra,
    data: { productId: kolumbie.id, quantity: 1, intervalDays: 14, firstDeliveryOn: today() },
  })
  expect(created.ok()).toBeTruthy()
  const { id } = await created.json()
  const setup = await (await request.post(`/api/subscriptions/${id}/card-setup`, { headers: petra })).json()
  const gateway = new URL(setup.redirectUrl).origin
  expect((await request.post(`${gateway}/setups/${setup.setupId}/approve`)).ok()).toBeTruthy()
  const activated = await request.get(`/api/subscriptions/${id}/card-return?setupId=${setup.setupId}`, { headers: petra })
  expect((await activated.json()).status).toBe('Active')
  return id as string
}

export function today() {
  const d = new Date()
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}
