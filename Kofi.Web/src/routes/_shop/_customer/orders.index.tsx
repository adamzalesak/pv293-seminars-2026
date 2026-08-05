import { createFileRoute, Link, useNavigate } from '@tanstack/react-router'
import { Package } from 'lucide-react'
import { z } from 'zod'
import { getGetOrdersQueryOptions, useGetOrders } from '@/api/generated/endpoints'
import { OrderStatus } from '@/components/order-status'
import { ErrorState } from '@/components/states'
import { Button } from '@/components/ui/button'
import { Empty, EmptyContent, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import { Switch } from '@/components/ui/switch'
import { formatDateTime, formatMoney } from '@/lib/format'

export const Route = createFileRoute('/_shop/_customer/orders/')({
  validateSearch: z.object({
    sortBy: z.enum(['date', 'total']).optional().catch(undefined),
    onlyPaid: z.boolean().optional().catch(undefined),
  }),
  loaderDeps: ({ search }) => search,
  loader: ({ context, deps }) => context.queryClient.prefetchQuery(getGetOrdersQueryOptions(params(deps))),
  component: MyOrders,
})

const params = (s: { sortBy?: string; onlyPaid?: boolean }) => ({ sortBy: s.sortBy, onlyPaid: s.onlyPaid })

function MyOrders() {
  const { sortBy, onlyPaid } = Route.useSearch()
  const navigate = useNavigate({ from: Route.fullPath })
  const orders = useGetOrders(params({ sortBy, onlyPaid }))

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between">
        <h1 className="text-3xl font-bold">Your orders</h1>
        <div className="flex items-center gap-5">
          <div className="flex items-center gap-2">
            <Switch
              id="only-paid"
              checked={!!onlyPaid}
              onCheckedChange={(v) => navigate({ search: (s) => ({ ...s, onlyPaid: v || undefined }) })}
            />
            <Label htmlFor="only-paid">Paid only</Label>
          </div>
          <Select value={sortBy ?? 'date'} onValueChange={(v) => navigate({ search: (s) => ({ ...s, sortBy: v === 'date' ? undefined : 'total' }) })}>
            <SelectTrigger className="w-40" aria-label="Sort by">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="date">Newest first</SelectItem>
              <SelectItem value="total">Highest total</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {orders.isError ? (
        <ErrorState error={orders.error} onRetry={() => orders.refetch()} />
      ) : orders.isPending ? (
        <Skeleton className="h-64 rounded-xl" />
      ) : orders.data.length === 0 ? (
        <Empty className="border py-16">
          <EmptyHeader>
            <EmptyMedia variant="icon">
              <Package />
            </EmptyMedia>
            <EmptyTitle>{onlyPaid ? 'No paid orders yet' : 'No orders yet'}</EmptyTitle>
            <EmptyDescription>Orders you place show up here, with their payment and delivery status.</EmptyDescription>
          </EmptyHeader>
          <EmptyContent>
            <Button asChild>
              <Link to="/" search={{}}>Browse the shop</Link>
            </Button>
          </EmptyContent>
        </Empty>
      ) : (
        <ul className="divide-y rounded-xl border bg-card">
          {orders.data.map((o) => (
            <li key={o.id}>
              <Link
                to="/orders/$orderId"
                params={{ orderId: o.id! }}
                className="flex flex-wrap items-center gap-x-6 gap-y-1 p-4 transition-colors hover:bg-muted/60"
              >
                <span className="font-medium tabular">#{o.id!.slice(0, 8)}</span>
                <span className="text-sm text-muted-foreground">{formatDateTime(o.createdAt!)}</span>
                <OrderStatus status={o.status} />
                <span className="ml-auto font-semibold tabular">{formatMoney(o.total ?? 0)}</span>
              </Link>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
