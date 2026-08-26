import { createFileRoute, Link } from '@tanstack/react-router'
import { CalendarClock } from 'lucide-react'
import { useState } from 'react'
import {
  getGetSubscriptionsQueryOptions,
  useGetSubscriptions,
  usePostSubscriptionsProcessDue,
} from '@/api/generated/endpoints'
import type { ProcessDueDeliveriesResponse } from '@/api/generated/model'
import { PageTitle } from '@/components/page-title'
import { ErrorState } from '@/components/states'
import { SubscriptionStatus } from '@/components/subscription-status'
import { Alert, AlertDescription, AlertTitle } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Spinner } from '@/components/ui/spinner'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { formatDate } from '@/lib/format'
import { useOnDone } from '@/lib/use-on-done'

export const Route = createFileRoute('/admin/subscriptions/')({
  loader: ({ context }) => context.queryClient.prefetchQuery(getGetSubscriptionsQueryOptions()),
  component: AdminSubscriptions,
})

function AdminSubscriptions() {
  const subscriptions = useGetSubscriptions()
  const onDone = useOnDone()
  const [result, setResult] = useState<ProcessDueDeliveriesResponse | null>(null)
  const processDue = usePostSubscriptionsProcessDue({
    mutation: { ...onDone('Due deliveries processed', '/subscriptions', '/orders', '/products'), onMutate: () => setResult(null) },
  })

  return (
    <>
      <PageTitle
        title="Subscriptions"
        description="Incomplete ones wait for the customer to authorise a card. Due deliveries are prepared by the daily job, or from here."
        actions={
          <Button
            variant="outline"
            disabled={processDue.isPending}
            onClick={() => processDue.mutate(undefined, { onSuccess: setResult })}
          >
            {processDue.isPending ? <Spinner /> : <CalendarClock />}
            Prepare due deliveries
          </Button>
        }
      />

      {result && (
        <Alert className="mb-5" variant={result.failed.length ? 'destructive' : 'default'}>
          <AlertTitle>
            {result.prepared.length} {result.prepared.length === 1 ? 'delivery' : 'deliveries'} prepared
            {result.failed.length > 0 && `, ${result.failed.length} failed`}
          </AlertTitle>
          {result.failed.length > 0 && (
            <AlertDescription>
              <ul className="list-disc pl-4">
                {result.failed.map((f) => (
                  <li key={f.deliveryId}>{f.reason}</li>
                ))}
              </ul>
            </AlertDescription>
          )}
        </Alert>
      )}

      {subscriptions.isError ? (
        <ErrorState error={subscriptions.error} onRetry={() => subscriptions.refetch()} />
      ) : subscriptions.isPending ? (
        <Skeleton className="h-72 rounded-xl" />
      ) : (
        <div className="rounded-xl border bg-card">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Customer</TableHead>
                <TableHead>Coffee</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Next delivery</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {subscriptions.data.map((s) => (
                <TableRow key={s.id}>
                  <TableCell>
                    <Link
                      to="/admin/subscriptions/$subscriptionId"
                      params={{ subscriptionId: s.id }}
                      className="font-medium hover:underline"
                    >
                      {s.customerName}
                    </Link>
                  </TableCell>
                  <TableCell>{s.productName}</TableCell>
                  <TableCell>
                    <SubscriptionStatus status={s.status} />
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {s.status === 'Active' && s.nextDeliveryAt ? formatDate(s.nextDeliveryAt) : '—'}
                  </TableCell>
                </TableRow>
              ))}
              {subscriptions.data.length === 0 && (
                <TableRow>
                  <TableCell colSpan={4} className="py-10 text-center text-muted-foreground">
                    No subscriptions yet.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </div>
      )}
    </>
  )
}
