import { createFileRoute, Link } from '@tanstack/react-router'
import { ArrowLeft, PackageOpen } from 'lucide-react'
import {
  getGetSubscriptionsIdQueryOptions,
  useGetSubscriptionsId,
  usePostSubscriptionsIdDeliveriesDeliveryIdPrepare,
} from '@/api/generated/endpoints'
import type { SubscriptionDeliveryDetail } from '@/api/generated/model'
import { PageTitle } from '@/components/page-title'
import { ErrorState } from '@/components/states'
import { SubscriptionDeliveries } from '@/components/subscription-deliveries'
import { SubscriptionStatus } from '@/components/subscription-status'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { formatDate, formatDateTime, formatMoney } from '@/lib/format'
import { intervalLabel, isDue } from '@/lib/subscriptions'
import { useOnDone } from '@/lib/use-on-done'

export const Route = createFileRoute('/admin/subscriptions/$subscriptionId')({
  loader: ({ context, params }) =>
    context.queryClient.prefetchQuery(getGetSubscriptionsIdQueryOptions(params.subscriptionId)),
  component: AdminSubscription,
})

function AdminSubscription() {
  const { subscriptionId } = Route.useParams()
  const subscription = useGetSubscriptionsId(subscriptionId)
  const onDone = useOnDone()
  const prepare = usePostSubscriptionsIdDeliveriesDeliveryIdPrepare({
    mutation: onDone('Delivery prepared', '/subscriptions', '/orders', '/products'),
  })

  if (subscription.isPending) return <Skeleton className="h-96 rounded-xl" />
  if (subscription.isError) return <ErrorState error={subscription.error} onRetry={() => subscription.refetch()} />
  const s = subscription.data

  const deliveryActions = (d: SubscriptionDeliveryDetail) => {
    // A delivery ends with its order; shipping is the order's business.
    if (d.status === 'Ready' && d.orderId && d.orderStatus !== 'Shipped')
      return (
        <Button variant="outline" size="sm" asChild>
          <Link to="/admin/orders/$orderId" params={{ orderId: d.orderId }}>
            Ship order
          </Link>
        </Button>
      )
    if (d.status !== 'Scheduled' || s.status !== 'Active') return null
    // The shop refuses to prepare a cycle before its date.
    if (!isDue(d.scheduledFor)) return <span className="text-sm text-muted-foreground">Not due yet</span>
    return (
      <Button
        size="sm"
        disabled={prepare.isPending}
        onClick={() => prepare.mutate({ id: s.id, deliveryId: d.id })}
      >
        <PackageOpen />
        Prepare
      </Button>
    )
  }

  return (
    <>
      <Button variant="ghost" size="sm" asChild className="mb-4 -ml-2">
        <Link to="/admin/subscriptions">
          <ArrowLeft />
          Subscriptions
        </Link>
      </Button>
      <PageTitle
        title={
          <span className="flex flex-wrap items-center gap-3">
            {s.productName}
            <SubscriptionStatus status={s.status} />
          </span>
        }
        description={`Created ${formatDateTime(s.createdAt)}`}
      />

      <div className="grid gap-6 lg:grid-cols-[1fr_20rem] lg:items-start">
        <Card>
          <CardHeader>
            <CardTitle>Deliveries</CardTitle>
          </CardHeader>
          <CardContent>
            <SubscriptionDeliveries subscription={s} orderLink="/admin/orders/$orderId" actions={deliveryActions} />
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Plan</CardTitle>
          </CardHeader>
          <CardContent>
            <dl className="grid grid-cols-[7rem_1fr] gap-y-3 text-sm">
              <dt className="text-muted-foreground">Customer</dt>
              <dd>
                <Link
                  to="/admin/customers/$customerId"
                  params={{ customerId: s.customerId }}
                  className="underline underline-offset-4"
                >
                  View customer
                </Link>
              </dd>
              <dt className="text-muted-foreground">Coffee</dt>
              <dd>
                {s.quantity} × {s.productName}
              </dd>
              <dt className="text-muted-foreground">Unit price</dt>
              <dd className="tabular">{formatMoney(s.unitPrice)}</dd>
              <dt className="text-muted-foreground">How often</dt>
              <dd>{intervalLabel(s.intervalDays)}</dd>
              {s.activatedAt && (
                <>
                  <dt className="text-muted-foreground">Activated</dt>
                  <dd>{formatDate(s.activatedAt)}</dd>
                </>
              )}
              {(s.status === 'Active' || s.status === 'Incomplete') && s.nextDeliveryAt && (
                <>
                  <dt className="text-muted-foreground">{s.status === 'Incomplete' ? 'First delivery' : 'Next delivery'}</dt>
                  <dd>{formatDate(s.nextDeliveryAt)}</dd>
                </>
              )}
              {s.pausedAt && s.status === 'Paused' && (
                <>
                  <dt className="text-muted-foreground">Paused</dt>
                  <dd>{formatDate(s.pausedAt)}</dd>
                </>
              )}
              {s.cancelledAt && (
                <>
                  <dt className="text-muted-foreground">Cancelled</dt>
                  <dd>{formatDate(s.cancelledAt)}</dd>
                </>
              )}
            </dl>
          </CardContent>
        </Card>
      </div>
    </>
  )
}
