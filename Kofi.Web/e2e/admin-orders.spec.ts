import { expect, test, type APIRequestContext } from '@playwright/test'
import { signIn } from './helpers'

const GATEWAY = process.env.KOFI_GATEWAY_URL ?? 'http://localhost:5299'

async function token(request: APIRequestContext, email: string, password: string) {
  const login = await request.post('/api/auth/login', { data: { email, password } })
  return { Authorization: `Bearer ${(await login.json()).accessToken}` }
}

async function placeOrder(request: APIRequestContext) {
  const petra = await token(request, 'petra@example.com', 'Customer123!')
  const kolumbie = (await (await request.get('/api/products?search=Kolumbie')).json()).items[0]
  const lungo = (await (await request.get('/api/products?search=Lungo')).json()).items[0]
  const order = await request.post('/api/orders', {
    headers: petra,
    data: { customerName: 'Petra Nováková', street: 'Kounicova 12', city: 'Brno', zip: '60200', items: [{ productId: kolumbie.id, quantity: 1 }, { productId: lungo.id, quantity: 1 }] },
  })
  return (await order.json()).id as string
}

test('an administrator edits and cancels a new order', async ({ page, request }) => {
  const orderId = await placeOrder(request)
  await signIn(page, 'admin@kofi.test', 'Admin123!')
  await page.goto(`/admin/orders/${orderId}`)

  await page.getByRole('button', { name: 'Remove Kapsle Lungo Delicato 10 ks' }).click()
  await expect(page.getByRole('cell', { name: 'Kapsle Lungo Delicato 10 ks' })).toHaveCount(0)

  await page.getByRole('button', { name: 'Change address' }).click()
  await page.getByLabel('Street').fill('Veveří 1')
  await page.getByRole('button', { name: 'Save address' }).click()
  await expect(page.getByText('Veveří 1')).toBeVisible()

  await page.getByRole('button', { name: 'Resend confirmation' }).click()
  await expect(page.getByText('Confirmation sent again')).toBeVisible()

  await page.getByRole('button', { name: 'Cancel order' }).click()
  await page.getByRole('alertdialog').getByRole('button', { name: 'Cancel order' }).click()
  await expect(page.getByText('Cancelled', { exact: true })).toBeVisible()
})

test('a cancelled order stays cancelled and keeps no money', async ({ request }) => {
  const petra = await token(request, 'petra@example.com', 'Customer123!')
  const admin = await token(request, 'admin@kofi.test', 'Admin123!')
  const pay = async (id: string) => (await (await request.post(`/api/orders/${id}/pay`, { headers: petra })).json()).paymentId as string
  const payment = async (id: string) => (await (await request.get(`${GATEWAY}/payments/${id}`)).json()).status as string

  // Cancelled while the customer is still on the payment page: the page can no longer take money.
  const open = await placeOrder(request)
  const openPayment = await pay(open)
  expect((await request.post(`/api/orders/${open}/cancel`, { headers: admin })).ok()).toBeTruthy()
  await request.post(`${GATEWAY}/payments/${openPayment}/approve`)
  expect(await payment(openPayment)).toBe('Cancelled')

  // Paid at the provider just before the cancel: the money goes back.
  const paid = await placeOrder(request)
  const paidPayment = await pay(paid)
  await request.post(`${GATEWAY}/payments/${paidPayment}/approve`)
  expect((await request.post(`/api/orders/${paid}/cancel`, { headers: admin })).ok()).toBeTruthy()
  expect(await payment(paidPayment)).toBe('Refunded')
  const back = await request.get(`/api/orders/${paid}/payment-return?paymentId=${paidPayment}`, { headers: petra })
  expect((await back.json()).status).toBe('Cancelled')

  // Cancelling again or changing a cancelled order is refused.
  expect((await request.post(`/api/orders/${paid}/cancel`, { headers: admin })).ok()).toBeFalsy()
  const lungo = (await (await request.get('/api/products?search=Lungo')).json()).items[0]
  const add = await request.post(`/api/orders/${paid}/items`, { headers: admin, data: { productId: lungo.id, quantity: 1 } })
  expect(add.ok()).toBeFalsy()
  const detail = await (await request.get(`/api/orders/${paid}`, { headers: admin })).json()
  expect(detail.status).toBe('Cancelled')
})

test('an administrator ships a paid order, exports orders and makes a VIP', async ({ page }) => {
  await signIn(page, 'admin@kofi.test', 'Admin123!')
  await page.goto('/admin/orders?onlyPaid=true')
  const paid = page.getByRole('row').filter({ hasText: 'Paid' }).first()
  await paid.getByRole('link').click()
  await page.getByRole('button', { name: 'Mark as shipped' }).click()
  await expect(page.getByText('Shipped', { exact: true })).toBeVisible()

  await page.goto('/admin/orders')
  const download = page.waitForEvent('download')
  await page.getByRole('button', { name: 'Export CSV' }).click()
  expect((await download).suggestedFilename()).toBe('kofi-orders.csv')

  await page.goto('/admin/customers')
  await page.getByRole('link', { name: 'Jan Dvořák' }).click()
  await expect(page.getByRole('heading', { name: /Jan Dvořák/ })).toBeVisible()
  const makeVip = page.getByRole('button', { name: 'Make VIP' })
  if (await makeVip.isVisible()) await makeVip.click()
  await expect(page.getByText('VIP', { exact: true })).toBeVisible()
})

test('an administrator adds an item to a new order', async ({ page, request }) => {
  const orderId = await placeOrder(request)
  await signIn(page, 'admin@kofi.test', 'Admin123!')
  await page.goto(`/admin/orders/${orderId}`)
  await page.getByRole('button', { name: 'Add item' }).click()
  await page.getByRole('combobox', { name: 'Product' }).click()
  await page.getByRole('option', { name: /Konvice/ }).click()
  await page.getByRole('dialog').getByRole('button', { name: 'Add item' }).click()
  await expect(page.getByRole('cell', { name: 'Konvice s husím krkem 1 l', exact: true })).toBeVisible()
})
