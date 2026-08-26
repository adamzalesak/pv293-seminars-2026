import { useMutation } from '@tanstack/react-query'
import { createFileRoute, Link } from '@tanstack/react-router'
import { CreditCard, Repeat } from 'lucide-react'
import { getGetSubscriptionsQueryOptions, useGetPaymentCard, useGetSubscriptions } from '@/api/generated/endpoints'
import { ErrorState } from '@/components/states'
import { SubscriptionStatus } from '@/components/subscription-status'
import { Button } from '@/components/ui/button'
import { Empty, EmptyContent, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import { Skeleton } from '@/components/ui/skeleton'
import { changeCard } from '@/lib/authorise-card'
import { formatDate } from '@/lib/format'

export const Route = createFileRoute('/_shop/_customer/subscriptions/')({
  loader: ({ context }) => context.queryClient.prefetchQuery(getGetSubscriptionsQueryOptions()),
  component: MySubscriptions,
})

function MySubscriptions() {
  const subscriptions = useGetSubscriptions()
  const card = useGetPaymentCard()
  const change = useMutation({ mutationFn: changeCard })

  return (
    <div className="space-y-6">
      <h1 className="text-3xl font-bold">Your subscriptions</h1>

      {card.data?.label && (
        <div className="flex flex-wrap items-center gap-x-4 gap-y-2 rounded-xl border bg-card p-4 text-sm">
          <CreditCard className="size-4 text-muted-foreground" />
          <span>
            Every delivery is charged to <span className="font-medium">{card.data.label}</span>.
          </span>
          <Button variant="outline" size="sm" className="ml-auto" disabled={change.isPending} onClick={() => change.mutate()}>
            Change card
          </Button>
        </div>
      )}

      {subscriptions.isError ? (
        <ErrorState error={subscriptions.error} onRetry={() => subscriptions.refetch()} />
      ) : subscriptions.isPending ? (
        <Skeleton className="h-48 rounded-xl" />
      ) : subscriptions.data.length === 0 ? (
        <Empty className="border py-16">
          <EmptyHeader>
            <EmptyMedia variant="icon">
              <Repeat />
            </EmptyMedia>
            <EmptyTitle>No subscriptions yet</EmptyTitle>
            <EmptyDescription>
              Pick a coffee and have it delivered on a schedule. Pause, skip or cancel whenever you like.
            </EmptyDescription>
          </EmptyHeader>
          <EmptyContent>
            <Button asChild>
              <Link to="/" search={{}}>Browse coffee</Link>
            </Button>
          </EmptyContent>
        </Empty>
      ) : (
        <ul className="divide-y rounded-xl border bg-card">
          {subscriptions.data.map((s) => (
            <li key={s.id}>
              <Link
                to="/subscriptions/$subscriptionId"
                params={{ subscriptionId: s.id }}
                className="flex flex-wrap items-center gap-x-6 gap-y-1 p-4 transition-colors hover:bg-muted/60"
              >
                <span className="font-medium">{s.productName}</span>
                <SubscriptionStatus status={s.status} />
                <span className="ml-auto text-sm text-muted-foreground">
                  {s.nextDeliveryAt && s.status === 'Active'
                    ? `Next delivery ${formatDate(s.nextDeliveryAt)}`
                    : s.status === 'Incomplete'
                      ? 'Waiting for your card'
                      : ''}
                </span>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
