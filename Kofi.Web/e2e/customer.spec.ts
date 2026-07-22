import { expect, test } from '@playwright/test'
import { signIn } from './helpers'

test('a customer buys coffee and pays for it', async ({ page }) => {
  await signIn(page, 'petra@example.com', 'Customer123!')

  await page.goto('/')
  await page.getByRole('button', { name: 'Add Kolumbie Huila 250 g to cart' }).click()
  await page.getByRole('button', { name: 'Add Ruční mlýnek Kompakt to cart' }).click()
  await page.getByRole('button', { name: /Cart, 2 items/ }).click()
  await page.getByRole('dialog').getByRole('button', { name: 'Go to checkout' }).click()

  await page.getByLabel('Full name').fill('Petra Nováková')
  await page.getByLabel('Street and number').fill('Kounicova 12')
  await page.getByLabel('City').fill('Brno')
  await page.getByLabel('Postcode').fill('60200')
  await page.getByRole('button', { name: 'Place order and pay' }).click()

  // The payment provider's own page.
  await expect(page.getByRole('heading', { name: /Pay 99\.90 EUR/ })).toBeVisible()
  await page.getByRole('button', { name: 'Pay', exact: true }).click()

  await expect(page.getByText('Payment received')).toBeVisible()
  await page.getByRole('link', { name: 'View order' }).click()
  await expect(page.getByText('Paid', { exact: true })).toBeVisible()
})

test('a declined payment can be tried again', async ({ page }) => {
  await signIn(page, 'petra@example.com', 'Customer123!')

  await page.goto('/')
  await page.getByRole('button', { name: 'Add Kolumbie Huila 250 g to cart' }).click()
  await page.goto('/checkout')
  await page.getByLabel('Full name').fill('Petra Nováková')
  await page.getByLabel('Street and number').fill('Kounicova 12')
  await page.getByLabel('City').fill('Brno')
  await page.getByLabel('Postcode').fill('60200')
  await page.getByRole('button', { name: 'Place order and pay' }).click()

  await page.getByRole('button', { name: 'Decline' }).click()
  await expect(page.getByText('Payment declined')).toBeVisible()

  await page.getByRole('button', { name: 'Try paying again' }).click()
  await page.getByRole('button', { name: 'Pay', exact: true }).click()
  await expect(page.getByText('Payment received')).toBeVisible()
})
