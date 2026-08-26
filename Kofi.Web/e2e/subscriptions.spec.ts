import { expect, test } from '@playwright/test'
import { activeSubscription, signIn, today } from './helpers'

test('a customer subscribes, authorises a card and the administrator ships the first order', async ({ page, browser }) => {
  await signIn(page, 'petra@example.com', 'Customer123!')
  await page.goto('/?q=Kolumbie')
  await page.getByRole('link', { name: 'Kolumbie Huila 250 g' }).click()
  await page.getByRole('button', { name: 'Subscribe' }).click()
  await page.getByLabel('Bags per delivery').fill('2')
  // Petra has a saved card; here she authorises a different one.
  await page.getByRole('button', { name: 'Use a different card' }).click()
  await page.getByRole('button', { name: 'Continue to card' }).click()
  // The payment provider's own page: the customer authorises future charges.
  await page.getByRole('button', { name: 'Authorise' }).click()
  await expect(page.getByText('Subscription active')).toBeVisible()
  await page.getByRole('link', { name: 'View subscription' }).click()
  await expect(page.getByText('Active', { exact: true })).toBeVisible()
  const subscriptionUrl = new URL(page.url()).pathname

  const admin = await (await browser.newContext()).newPage()
  await signIn(admin, 'admin@kofi.test', 'Admin123!')
  await admin.goto(`/admin${subscriptionUrl}`)
  await expect(admin.getByText('Active', { exact: true })).toBeVisible()

  // The first delivery became a paid order at once; once the order ships,
  // the delivery shows the order's status.
  await admin.getByRole('link', { name: 'Ship order' }).click()
  await admin.getByRole('button', { name: 'Mark as shipped' }).click()
  await expect(admin.getByText('Shipped', { exact: true })).toBeVisible()
  await admin.goto(`/admin${subscriptionUrl}`)
  await expect(admin.getByText('Shipped', { exact: true })).toBeVisible()

  await page.reload()
  await expect(page.getByText('Active', { exact: true })).toBeVisible()
  await expect(page.getByText('Shipped', { exact: true })).toBeVisible()
})

test('a customer changes, pauses, resumes, skips and cancels a subscription', async ({ page, request }) => {
  const id = await activeSubscription(request)
  await signIn(page, 'petra@example.com', 'Customer123!')
  await page.goto(`/subscriptions/${id}`)

  await page.getByRole('button', { name: 'Add extra' }).click()
  await page.getByRole('combobox', { name: 'Product' }).click()
  await page.getByRole('option', { name: /Lungo/ }).click()
  await page.getByRole('button', { name: 'Add to delivery' }).click()
  await expect(page.getByText(/\+ 1 × Kapsle Lungo/)).toBeVisible()
  await page.getByRole('button', { name: /Remove Kapsle Lungo/ }).click()
  await expect(page.getByText(/\+ 1 × Kapsle Lungo/)).toHaveCount(0)

  await page.getByRole('button', { name: 'Change plan' }).click()
  await page.getByLabel('Bags per delivery').fill('3')
  await page.getByRole('combobox', { name: 'Delivery interval' }).click()
  await page.getByRole('option', { name: 'Every 4 weeks' }).click()
  await page.getByRole('button', { name: 'Save plan' }).click()
  await expect(page.getByRole('dialog')).toBeHidden()
  await expect(page.getByText('Every 4 weeks')).toBeVisible()

  await page.getByRole('button', { name: 'Pause' }).click()
  await expect(page.getByText('Paused', { exact: true })).toBeVisible()
  await page.getByRole('button', { name: 'Resume' }).click()
  await page.getByLabel('Next delivery').fill(today())
  await page.getByRole('button', { name: 'Resume subscription' }).click()
  await expect(page.getByText('Active', { exact: true })).toBeVisible()

  await page.getByRole('button', { name: 'Skip next delivery' }).click()
  await expect(page.getByText('Skipped', { exact: true })).toBeVisible()

  await page.getByRole('button', { name: 'Cancel subscription' }).click()
  await page.getByRole('alertdialog').getByRole('button', { name: 'Cancel subscription' }).click()
  await expect(page.getByText('Cancelled', { exact: true }).first()).toBeVisible()

  await page.goto('/subscriptions')
  await expect(page.getByRole('link', { name: /Kolumbie Huila/ }).first()).toBeVisible()
})

test('an administrator prepares due deliveries', async ({ page }) => {
  await signIn(page, 'admin@kofi.test', 'Admin123!')
  await page.goto('/admin/subscriptions')
  await expect(page.getByRole('cell', { name: 'Jan Dvořák' }).first()).toBeVisible()
  await page.getByRole('button', { name: 'Prepare due deliveries' }).click()
  await expect(page.getByText(/deliver(y|ies) prepared/)).toBeVisible()
})
