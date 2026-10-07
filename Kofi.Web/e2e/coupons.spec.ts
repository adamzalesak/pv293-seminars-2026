import { expect, test } from '@playwright/test'
import { authHeader, signIn } from './helpers'

test('the administrator issues, edits and invalidates a coupon', async ({ page }) => {
  const code = `E2E${Date.now().toString(36).toUpperCase()}`
  await signIn(page, 'admin@kofi.test', 'Admin123!')
  await page.goto('/admin/coupons')

  await page.getByRole('button', { name: 'New coupon' }).click()
  await page.getByLabel('Code').fill(code)
  await page.getByLabel('Discount (%)').fill('20')
  await page.getByRole('button', { name: 'Issue coupon' }).click()
  const row = page.getByRole('row', { name: new RegExp(code) })
  await expect(row).toContainText('20 %')

  await page.getByRole('button', { name: `Actions for ${code}` }).click()
  await page.getByRole('menuitem', { name: 'Edit' }).click()
  await page.getByLabel('Discount (%)').fill('25')
  await page.getByRole('button', { name: 'Save changes' }).click()
  await expect(row).toContainText('25 %')

  await page.getByRole('button', { name: `Actions for ${code}` }).click()
  await page.getByRole('menuitem', { name: 'Invalidate' }).click()
  await page.getByRole('button', { name: 'Invalidate' }).click()
  await expect(row).toContainText('Invalidated')
})

test('a customer pays less with a coupon and sees her referral code', async ({ page, request }) => {
  const code = `E2E${Date.now().toString(36).toUpperCase()}`
  const admin = await authHeader(request, 'admin@kofi.test', 'Admin123!')
  const expiresAt = new Date(Date.now() + 7 * 86_400_000).toISOString().slice(0, 19)
  expect((await request.post('/api/coupons', { headers: admin, data: { code, discountPercent: 10, expiresAt } })).ok()).toBeTruthy()

  await signIn(page, 'petra@example.com', 'Customer123!')
  await page.goto('/coupons')
  await expect(page.getByText('REF-PETRA7')).toBeVisible()

  // A product no other test orders, so parallel tests do not reserve the same stock.
  await page.goto('/?q=Espresso')
  await page.getByRole('button', { name: 'Add Kapsle Espresso Forte 10 ks to cart' }).click()
  await page.getByRole('button', { name: /Cart, 1 item/ }).click()
  await page.getByRole('dialog').getByRole('button', { name: 'Go to checkout' }).click()
  await page.getByLabel('Full name').fill('Petra Nováková')
  await page.getByLabel('Street and number').fill('Kounicova 12')
  await page.getByLabel('City').fill('Brno')
  await page.getByLabel('Postcode').fill('602 00')

  // Her own referral code is for friends.
  await page.getByLabel('Coupon code').fill('REF-PETRA7')
  await page.getByRole('button', { name: 'Apply' }).click()
  await expect(page.getByText('A referral code is for someone else.')).toBeVisible()

  await page.getByLabel('Coupon code').fill(code)
  await page.getByRole('button', { name: 'Apply' }).click()
  await expect(page.getByText(`Coupon ${code}`)).toBeVisible()
  await page.getByRole('button', { name: 'Place order and pay' }).click()
  await expect(page.getByRole('heading', { name: /Pay \d+\.\d{2} EUR/ })).toBeVisible()
  await page.getByRole('button', { name: 'Pay', exact: true }).click()
  await page.getByRole('link', { name: 'View order' }).click()
  await expect(page.getByText(`Coupon ${code}`)).toBeVisible()
})
