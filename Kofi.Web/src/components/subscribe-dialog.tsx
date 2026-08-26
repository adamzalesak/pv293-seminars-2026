import { useNavigate } from '@tanstack/react-router'
import { Repeat } from 'lucide-react'
import { useState } from 'react'
import { useGetPaymentCard, usePostSubscriptions, usePostSubscriptionsIdActivate } from '@/api/generated/endpoints'
import type { ProductDetail } from '@/api/generated/model'
import { IntervalSelect } from '@/components/interval-select'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import { Field, FieldGroup, FieldLabel } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { authoriseCard } from '@/lib/authorise-card'
import { formatMoney, isoDay } from '@/lib/format'
import { chargeNote, deliveryCharge } from '@/lib/subscriptions'
import { useOnDone } from '@/lib/use-on-done'

export function SubscribeDialog({ product }: { product: ProductDetail }) {
  const [open, setOpen] = useState(false)
  const [quantity, setQuantity] = useState(1)
  const [intervalDays, setIntervalDays] = useState(14)
  const [firstDeliveryOn, setFirstDeliveryOn] = useState(isoDay())
  const [newCard, setNewCard] = useState(false)
  const charge = deliveryCharge(Number(product.price) * quantity)
  const navigate = useNavigate()
  const onDone = useOnDone()
  // A card saved earlier already allows future charges, so the subscription
  // starts at once. Otherwise the customer authorises one at the provider.
  const savedCard = useGetPaymentCard().data?.label
  const chargeSaved = !!savedCard && !newCard
  const activate = usePostSubscriptionsIdActivate({ mutation: onDone('Subscription started', '/subscriptions', '/orders', '/products') })
  const subscribe = usePostSubscriptions({
    mutation: {
      onSuccess: (s) =>
        chargeSaved
          ? activate.mutate(
              { id: s.id },
              { onSettled: () => navigate({ to: '/subscriptions/$subscriptionId', params: { subscriptionId: s.id } }) },
            )
          : authoriseCard(s.id),
    },
  })

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" size="lg">
          <Repeat />
          Subscribe
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Subscribe to {product.name}</DialogTitle>
          <DialogDescription>
            {chargeSaved ? (
              <>
                Delivered on a schedule and charged to {savedCard} when we prepare each delivery.{' '}
                <button type="button" className="underline" onClick={() => setNewCard(true)}>
                  Use a different card
                </button>
              </>
            ) : savedCard ? (
              `Delivered on a schedule. Next you authorise a new card with our payment provider; it replaces ${savedCard} for all your subscriptions.`
            ) : (
              'Delivered on a schedule. Next you authorise a card with our payment provider; each delivery is charged to it when we prepare it.'
            )}{' '}
            Pause, skip or cancel any time.
          </DialogDescription>
        </DialogHeader>
        <form
          onSubmit={(e) => {
            e.preventDefault()
            subscribe.mutate({ data: { productId: product.id, quantity, intervalDays, firstDeliveryOn } })
          }}
        >
          <FieldGroup>
            <Field>
              <FieldLabel htmlFor="subscribe-qty">Bags per delivery</FieldLabel>
              <Input
                id="subscribe-qty"
                type="number"
                min={1}
                max={100}
                value={quantity}
                onChange={(e) => setQuantity(Number(e.target.value))}
              />
            </Field>
            <Field>
              <FieldLabel>How often</FieldLabel>
              <IntervalSelect value={intervalDays} onChange={setIntervalDays} />
            </Field>
            <Field>
              <FieldLabel htmlFor="subscribe-first">First delivery</FieldLabel>
              <Input
                id="subscribe-first"
                type="date"
                min={isoDay()}
                max={isoDay(90)}
                value={firstDeliveryOn}
                onChange={(e) => setFirstDeliveryOn(e.target.value)}
              />
            </Field>
          </FieldGroup>
          <DialogFooter className="mt-6 items-center sm:justify-between">
            <span className="text-sm text-muted-foreground tabular">
              {formatMoney(charge.total)} per delivery, {chargeNote(charge.shipping)}
            </span>
            <Button type="submit" disabled={quantity < 1 || !firstDeliveryOn || subscribe.isPending || activate.isPending}>
              {chargeSaved ? 'Start subscription' : 'Continue to card'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
