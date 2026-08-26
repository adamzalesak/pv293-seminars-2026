import { postPaymentCardSetup, postSubscriptionsIdCardSetup } from '@/api/generated/endpoints'

// The provider asks the customer to authorise future charges on its own page
// and later sends them back to /subscriptions/{id}/card-return on this site.
export async function authoriseCard(subscriptionId: string) {
  const setup = await postSubscriptionsIdCardSetup(subscriptionId)
  window.location.assign(setup.redirectUrl!)
}

// Replaces the saved card for every subscription; the provider sends the
// customer back to /payment-card/return.
export async function changeCard() {
  const setup = await postPaymentCardSetup()
  window.location.assign(setup.redirectUrl!)
}
