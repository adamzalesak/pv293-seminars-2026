import { createFileRoute, Link } from '@tanstack/react-router'
import {
  getGetOrdersQueryOptions,
  getGetOrdersStatsStatusQueryOptions,
  useGetOrders,
  useGetOrdersStatsStatus,
} from '@/api/generated/endpoints'
import { OrderStatus } from '@/components/order-status'
import { PageTitle } from '@/components/page-title'
import { ErrorState } from '@/components/states'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { formatDateTime, formatMoney } from '@/lib/format'

export const Route = createFileRoute('/admin/')({
  loader: ({ context: { queryClient } }) =>
    Promise.all([
      queryClient.prefetchQuery(getGetOrdersQueryOptions({ sortBy: 'date' })),
      ...stats.map((s) => queryClient.prefetchQuery(getGetOrdersStatsStatusQueryOptions(s.status))),
    ]),
  component: Overview,
})

const stats = [
  { status: 'New', label: 'Awaiting payment' },
  { status: 'Paid', label: 'To ship' },
  { status: 'Shipped', label: 'Shipped' },
  { status: 'Cancelled', label: 'Cancelled' },
]

function Stat({ status, label }: { status: string; label: string }) {
  const count = useGetOrdersStatsStatus(status)
  return (
    <div className="rounded-xl border bg-card p-5">
      <div className="text-sm text-muted-foreground">{label}</div>
      <div className="mt-2 font-heading text-4xl font-bold tabular">
        {count.isPending ? <Skeleton className="h-10 w-12" /> : count.isError ? '—' : count.data.count}
      </div>
    </div>
  )
}

function Overview() {
  const orders = useGetOrders({ sortBy: 'date' })

  return (
    <>
      <PageTitle title="Overview" description="Orders by where they are right now." />
      <div className="grid grid-cols-2 gap-4 lg:grid-cols-4">
        {stats.map((s) => (
          <Stat key={s.status} {...s} />
        ))}
      </div>
      <div className="mt-10 mb-4 flex items-center justify-between">
        <h2 className="text-xl font-semibold">Latest orders</h2>
        <Button variant="outline" size="sm" asChild>
          <Link to="/admin/orders">All orders</Link>
        </Button>
      </div>
      {orders.isError ? (
        <ErrorState error={orders.error} onRetry={() => orders.refetch()} />
      ) : orders.isPending ? (
        <Skeleton className="h-48 rounded-xl" />
      ) : (
        <div className="rounded-xl border bg-card">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Order</TableHead>
                <TableHead>Customer</TableHead>
                <TableHead>Placed</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Total</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {orders.data.slice(0, 6).map((o) => (
                <TableRow key={o.id}>
                  <TableCell>
                    <Link to="/admin/orders/$orderId" params={{ orderId: o.id! }} className="font-medium tabular hover:underline">
                      #{o.id!.slice(0, 8)}
                    </Link>
                  </TableCell>
                  <TableCell>{o.customerName}</TableCell>
                  <TableCell className="text-muted-foreground">{formatDateTime(o.createdAt!)}</TableCell>
                  <TableCell>
                    <OrderStatus status={o.status} />
                  </TableCell>
                  <TableCell className="text-right tabular">{formatMoney(o.total ?? 0)}</TableCell>
                </TableRow>
              ))}
              {orders.data.length === 0 && (
                <TableRow>
                  <TableCell colSpan={5} className="py-10 text-center text-muted-foreground">
                    No orders yet. They appear here as soon as a customer checks out.
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
