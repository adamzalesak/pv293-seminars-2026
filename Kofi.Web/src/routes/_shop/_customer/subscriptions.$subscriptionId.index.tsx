import { createFileRoute, Link } from '@tanstack/react-router'
import { ArrowLeft, CreditCard, Pause, Pencil, Play, Plus, SkipForward, X, XCircle } from 'lucide-react'
import { useMutation } from '@tanstack/react-query'
import { useState } from 'react'
import {
  getGetSubscriptionsIdQueryOptions,
  useDeleteSubscriptionsIdDeliveriesDeliveryIdExtrasProductId,
  useGetProducts,
  useGetSubscriptionsId,
  usePostSubscriptionsIdCancel,
  usePostSubscriptionsIdDeliveriesDeliveryIdExtras,
  usePostSubscriptionsIdPause,
  usePostSubscriptionsIdResume,
  usePostSubscriptionsIdSkip,
  usePutSubscriptionsIdPlan,
} from '@/api/generated/endpoints'
import type { SubscriptionDetail } from '@/api/generated/model'
import { IntervalSelect } from '@/components/interval-select'
import { ErrorState } from '@/components/states'
import { SubscriptionDeliveries } from '@/components/subscription-deliveries'
import { SubscriptionStatus } from '@/components/subscription-status'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
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
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import { authoriseCard } from '@/lib/authorise-card'
import { formatDate, formatMoney, isoDay } from '@/lib/format'
import { chargeNote, deliveryCharge, intervalLabel } from '@/lib/subscriptions'
import { useOnDone } from '@/lib/use-on-done'

export const Route = createFileRoute('/_shop/_customer/subscriptions/$subscriptionId/')({
  loader: ({ context, params }) =>
    context.queryClient.prefetchQuery(getGetSubscriptionsIdQueryOptions(params.subscriptionId)),
  component: SubscriptionPage,
})

const statusNote: Record<string, string> = {
  Incomplete: 'Authorise a card with our payment provider to start. Nothing is planned or charged until then.',
  Active: 'Each delivery is charged to your saved card when we prepare it.',
  Paused: 'Paused. Nothing is prepared or charged until you resume.',
  Cancelled: 'This subscription is cancelled. Orders already on their way still arrive.',
}

function SubscriptionPage() {
  const { subscriptionId } = Route.useParams()
  const subscription = useGetSubscriptionsId(subscriptionId)
  const onDone = useOnDone()
  const pause = usePostSubscriptionsIdPause({ mutation: onDone('Subscription paused', '/subscriptions') })
  const skip = usePostSubscriptionsIdSkip({ mutation: onDone('Next delivery skipped', '/subscriptions') })
  const cancel = usePostSubscriptionsIdCancel({ mutation: onDone('Subscription cancelled', '/subscriptions') })
  const authorise = useMutation({ mutationFn: () => authoriseCard(subscriptionId) })
  const removeExtra = useDeleteSubscriptionsIdDeliveriesDeliveryIdExtrasProductId({
    mutation: onDone('Extra removed', '/subscriptions'),
  })

  if (subscription.isPending) return <Skeleton className="h-96 rounded-xl" />
  if (subscription.isError) return <ErrorState error={subscription.error} onRetry={() => subscription.refetch()} />
  const s = subscription.data
  const running = s.status === 'Active' || s.status === 'Paused'
  const id = { id: subscriptionId }
  const charge = deliveryCharge(Number(s.unitPrice) * Number(s.quantity))

  return (
    <div className="space-y-6">
      <Button variant="ghost" size="sm" asChild className="-ml-2">
        <Link to="/subscriptions">
          <ArrowLeft />
          All subscriptions
        </Link>
      </Button>
      <div className="flex flex-wrap items-center gap-3">
        <h1 className="text-3xl font-bold">{s.productName}</h1>
        <SubscriptionStatus status={s.status} />
      </div>
      <p className="text-muted-foreground">{statusNote[s.status]}</p>

      <div className="flex flex-wrap gap-2">
        {s.status === 'Incomplete' && (
          <Button disabled={authorise.isPending} onClick={() => authorise.mutate()}>
            <CreditCard />
            Authorise card
          </Button>
        )}
        {s.status === 'Active' && (
          <>
            <Button variant="outline" disabled={skip.isPending} onClick={() => skip.mutate(id)}>
              <SkipForward />
              Skip next delivery
            </Button>
            <Button variant="outline" disabled={pause.isPending} onClick={() => pause.mutate(id)}>
              <Pause />
              Pause
            </Button>
          </>
        )}
        {s.status === 'Paused' && <ResumeDialog subscriptionId={s.id} />}
        {running && <ChangePlanDialog subscription={s} />}
        {s.status !== 'Cancelled' && (
          <AlertDialog>
            <AlertDialogTrigger asChild>
              <Button variant="outline">
                <XCircle />
                Cancel subscription
              </Button>
            </AlertDialogTrigger>
            <AlertDialogContent>
              <AlertDialogHeader>
                <AlertDialogTitle>Cancel this subscription?</AlertDialogTitle>
                <AlertDialogDescription>
                  No more deliveries are planned. An order that’s already prepared still arrives.
                </AlertDialogDescription>
              </AlertDialogHeader>
              <AlertDialogFooter>
                <AlertDialogCancel>Keep subscription</AlertDialogCancel>
                <AlertDialogAction variant="destructive" onClick={() => cancel.mutate(id)}>
                  Cancel subscription
                </AlertDialogAction>
              </AlertDialogFooter>
            </AlertDialogContent>
          </AlertDialog>
        )}
      </div>

      <div className="grid gap-6 lg:grid-cols-[1fr_20rem] lg:items-start">
        <Card>
          <CardHeader>
            <CardTitle>Deliveries</CardTitle>
          </CardHeader>
          <CardContent>
            <SubscriptionDeliveries
              subscription={s}
              orderLink="/orders/$orderId"
              actions={(d) => running && d.status === 'Scheduled' && <AddExtraDialog subscriptionId={s.id} deliveryId={d.id} />}
              extraActions={(d, e) =>
                running &&
                d.status === 'Scheduled' && (
                  <Button
                    variant="ghost"
                    size="icon-sm"
                    aria-label={`Remove ${e.productName}`}
                    disabled={removeExtra.isPending}
                    onClick={() => removeExtra.mutate({ id: s.id, deliveryId: d.id, productId: e.productId })}
                  >
                    <X />
                  </Button>
                )
              }
            />
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Plan</CardTitle>
          </CardHeader>
          <CardContent>
            <dl className="grid grid-cols-[7rem_1fr] gap-y-3 text-sm">
              <dt className="text-muted-foreground">Coffee</dt>
              <dd>
                {s.quantity} × {s.productName}
              </dd>
              <dt className="text-muted-foreground">Price</dt>
              <dd className="tabular">
                {formatMoney(charge.total)} per delivery, {chargeNote(charge.shipping)}
              </dd>
              <dt className="text-muted-foreground">How often</dt>
              <dd>{intervalLabel(s.intervalDays)}</dd>
              {(s.status === 'Active' || s.status === 'Incomplete') && s.nextDeliveryAt && (
                <>
                  <dt className="text-muted-foreground">{s.status === 'Incomplete' ? 'First delivery' : 'Next delivery'}</dt>
                  <dd>{formatDate(s.nextDeliveryAt)}</dd>
                </>
              )}
              <dt className="text-muted-foreground">Since</dt>
              <dd>{formatDate(s.activatedAt ?? s.createdAt)}</dd>
            </dl>
          </CardContent>
        </Card>
      </div>
    </div>
  )
}

function ResumeDialog({ subscriptionId }: { subscriptionId: string }) {
  const [open, setOpen] = useState(false)
  const [nextDeliveryOn, setNextDeliveryOn] = useState(isoDay())
  const onDone = useOnDone()
  const resume = usePostSubscriptionsIdResume({ mutation: onDone('Subscription resumed', '/subscriptions') })

  return (
    <Dialog
      open={open}
      onOpenChange={(o) => {
        setOpen(o)
        if (o) setNextDeliveryOn(isoDay())
      }}
    >
      <DialogTrigger asChild>
        <Button>
          <Play />
          Resume
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Resume subscription</DialogTitle>
          <DialogDescription>
            Pick the day of your next delivery. Choose today and it goes out with today’s deliveries; the plan
            continues from there.
          </DialogDescription>
        </DialogHeader>
        <form
          onSubmit={(e) => {
            e.preventDefault()
            resume.mutate({ id: subscriptionId, data: { nextDeliveryOn } }, { onSuccess: () => setOpen(false) })
          }}
        >
          <FieldGroup>
            <Field>
              <FieldLabel htmlFor="resume-next">Next delivery</FieldLabel>
              <Input
                id="resume-next"
                type="date"
                min={isoDay()}
                max={isoDay(90)}
                value={nextDeliveryOn}
                onChange={(e) => setNextDeliveryOn(e.target.value)}
              />
            </Field>
          </FieldGroup>
          <DialogFooter className="mt-6">
            <Button type="submit" disabled={!nextDeliveryOn || resume.isPending}>
              Resume subscription
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function ChangePlanDialog({ subscription }: { subscription: SubscriptionDetail }) {
  const [open, setOpen] = useState(false)
  const [productId, setProductId] = useState(subscription.productId)
  const [quantity, setQuantity] = useState(Number(subscription.quantity))
  const [intervalDays, setIntervalDays] = useState(Number(subscription.intervalDays))
  const onDone = useOnDone()
  const products = useGetProducts({ pageSize: 100 }, { query: { enabled: open } })
  const coffees = products.data?.items.filter((p) => p.roastedOn) ?? []
  const save = usePutSubscriptionsIdPlan({ mutation: onDone('Plan changed', '/subscriptions') })

  return (
    <Dialog
      open={open}
      onOpenChange={(o) => {
        setOpen(o)
        if (o) {
          setProductId(subscription.productId)
          setQuantity(Number(subscription.quantity))
          setIntervalDays(Number(subscription.intervalDays))
        }
      }}
    >
      <DialogTrigger asChild>
        <Button variant="outline">
          <Pencil />
          Change plan
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Change plan</DialogTitle>
          <DialogDescription>
            Your next delivery keeps its date and gets the new coffee and quantity. A new interval counts from
            that delivery on.
          </DialogDescription>
        </DialogHeader>
        <form
          onSubmit={(e) => {
            e.preventDefault()
            save.mutate(
              { id: subscription.id, data: { productId, quantity, intervalDays } },
              { onSuccess: () => setOpen(false) },
            )
          }}
        >
          <FieldGroup>
            <Field>
              <FieldLabel>Coffee</FieldLabel>
              <Select value={productId} onValueChange={setProductId}>
                <SelectTrigger className="w-full" aria-label="Coffee">
                  <SelectValue placeholder={subscription.productName} />
                </SelectTrigger>
                <SelectContent>
                  {coffees.map((p) => (
                    <SelectItem key={p.id} value={p.id}>
                      {p.name} ({formatMoney(p.price)})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </Field>
            <Field>
              <FieldLabel htmlFor="plan-qty">Bags per delivery</FieldLabel>
              <Input
                id="plan-qty"
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
          </FieldGroup>
          <DialogFooter className="mt-6">
            <Button type="submit" disabled={quantity < 1 || save.isPending}>
              Save plan
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function AddExtraDialog({ subscriptionId, deliveryId }: { subscriptionId: string; deliveryId: string }) {
  const [open, setOpen] = useState(false)
  const [productId, setProductId] = useState('')
  const [quantity, setQuantity] = useState(1)
  const onDone = useOnDone()
  const products = useGetProducts({ pageSize: 100 }, { query: { enabled: open } })
  const add = usePostSubscriptionsIdDeliveriesDeliveryIdExtras({ mutation: onDone('Added to the delivery', '/subscriptions') })

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" size="sm">
          <Plus />
          Add extra
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Add to this delivery</DialogTitle>
        </DialogHeader>
        <form
          onSubmit={(e) => {
            e.preventDefault()
            add.mutate(
              { id: subscriptionId, deliveryId, data: { productId, quantity } },
              { onSuccess: () => setOpen(false) },
            )
          }}
        >
          <FieldGroup>
            <Field>
              <FieldLabel>Product</FieldLabel>
              <Select value={productId} onValueChange={setProductId}>
                <SelectTrigger className="w-full" aria-label="Product">
                  <SelectValue placeholder="Choose a product" />
                </SelectTrigger>
                <SelectContent>
                  {products.data?.items.map((p) => (
                    <SelectItem key={p.id} value={p.id}>
                      {p.name} ({formatMoney(p.price)})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </Field>
            <Field>
              <FieldLabel htmlFor="extra-qty">Quantity</FieldLabel>
              <Input
                id="extra-qty"
                type="number"
                min={1}
                value={quantity}
                onChange={(e) => setQuantity(Number(e.target.value))}
              />
            </Field>
          </FieldGroup>
          <DialogFooter className="mt-6">
            <Button type="submit" disabled={!productId || quantity < 1 || add.isPending}>
              Add to delivery
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
