import { expect, test } from '@playwright/test'

test('the cart drawer opens from the toast and changes quantities', async ({ page }) => {
  await page.goto('/')
  await page.getByRole('button', { name: 'Add Kolumbie Huila 250 g to cart' }).click()
  await page.getByRole('button', { name: 'View cart' }).click()

  const drawer = page.getByRole('dialog')
  await expect(drawer.getByText('1 item')).toBeVisible()
  await drawer.getByRole('button', { name: 'More Kolumbie Huila 250 g' }).click()
  await expect(drawer.getByText('2 items')).toBeVisible()

  await drawer.getByRole('button', { name: 'Remove' }).click()
  await expect(drawer.getByText('Your cart is empty')).toBeVisible()
  await page.keyboard.press('Escape')
  await expect(page.getByRole('button', { name: 'Cart, 0 items' })).toBeVisible()
})

test('back after searching, sorting and searching again restores the earlier search', async ({ page }) => {
  await page.goto('/')
  const search = page.getByRole('searchbox', { name: 'Search the shop' }).or(page.getByLabel('Search the shop'))
  await search.fill('Kolumbie')
  await expect(page).toHaveURL(/q=Kolumbie/)
  await page.getByRole('combobox', { name: 'Sort by' }).click()
  await page.getByRole('option', { name: 'Price, low to high' }).click()
  await expect(page).toHaveURL(/sort=price/)
  await search.fill('Lungo')
  await expect(page).toHaveURL(/q=Lungo/)

  await page.goBack()
  await expect(page).toHaveURL(/q=Kolumbie/)
  await expect(search).toHaveValue('Kolumbie')
  // Long enough for the search box to have written its own value back.
  await page.waitForTimeout(600)
  await expect(page).toHaveURL(/q=Kolumbie/)
})
