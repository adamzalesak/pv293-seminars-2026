import { expect, test } from '@playwright/test'

test('the shop sorts coffee by how many people subscribe to it', async ({ page, request }) => {
  const items = (await (await request.get('/api/products?sort=popular&pageSize=50')).json()).items
  const counts = items.map((p: { activeSubscriptionCount: number }) => Number(p.activeSubscriptionCount))
  expect(counts).toEqual([...counts].sort((a, b) => b - a))
  expect(counts[0]).toBeGreaterThan(0)

  await page.goto('/?sort=popular')
  await expect(page.getByRole('combobox', { name: 'Sort by' })).toHaveText('Most subscribed')
  const first = page.getByRole('heading', { level: 3 }).first()
  await expect(first).toHaveText(items[0].name)
  await first.getByRole('link').click()
  await expect(page.getByText('Subscribed by')).toBeVisible()
})
