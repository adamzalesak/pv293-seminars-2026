import type { SubscriptionDeliveryDetail } from '@/api/generated/model'
import { shippingFor } from '@/lib/cart'
import { formatMoney } from '@/lib/format'

// Intervals the shop offers when subscribing; the API accepts any from 1 to 365 days.
export const intervals = [
  { days: 7, label: 'Every week' },
  { days: 14, label: 'Every 2 weeks' },
  { days: 21, label: 'Every 3 weeks' },
  { days: 28, label: 'Every 4 weeks' },
]

export function intervalLabel(days: number | string) {
  const d = Number(days)
  return intervals.find((i) => i.days === d)?.label ?? `Every ${d} days`
}

// What one delivery is charged: the goods plus delivery, which is free from 50 EUR.
export function deliveryCharge(itemsTotal: number) {
  const shipping = shippingFor(itemsTotal)
  return { itemsTotal, shipping, total: itemsTotal + shipping }
}

export function deliveryTotal(d: SubscriptionDeliveryDetail) {
  return deliveryCharge(
    Number(d.unitPrice) * Number(d.quantity) +
      d.extras.reduce((sum, e) => sum + Number(e.unitPrice) * Number(e.quantity), 0),
  ).total
}

export const chargeNote = (shipping: number) =>
  shipping === 0 ? 'free delivery' : `incl. ${formatMoney(shipping)} delivery`

export const isDue = (scheduledFor: string) => new Date(scheduledFor) <= new Date()
