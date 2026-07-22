import { postOrdersIdPay } from '@/api/generated/endpoints'

// The provider takes the customer to its own page and later sends them back to
// /orders/{id}/payment-return on this site.
export async function payOrder(orderId: string) {
  const payment = await postOrdersIdPay(orderId)
  window.location.assign(payment.redirectUrl!)
}
