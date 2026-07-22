import { useMutation } from '@tanstack/react-query'
import { createFileRoute, Link } from '@tanstack/react-router'
import { ArrowLeft, CreditCard } from 'lucide-react'
import { getGetOrdersIdQueryOptions, useGetOrdersId } from '@/api/generated/endpoints'
import { OrderStatus } from '@/components/order-status'
import { OrderSummary } from '@/components/order-summary'
import { ErrorState } from '@/components/states'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { Spinner } from '@/components/ui/spinner'
import { formatDateTime, formatMoney } from '@/lib/format'
import { payOrder } from '@/lib/pay'

export const Route = createFileRoute('/_shop/_customer/orders/$orderId/')({
  loader: ({ context, params }) => context.queryClient.prefetchQuery(getGetOrdersIdQueryOptions(params.orderId)),
  component: OrderDetail,
})

const statusNote: Record<string, string> = {
  New: 'Waiting for payment. We start packing once it’s paid.',
  Paid: 'Paid. We’re packing it now.',
  Shipped: 'On its way. Check your e-mail for the delivery details.',
  Cancelled: 'This order was cancelled.',
}

function OrderDetail() {
  const { orderId } = Route.useParams()
  const order = useGetOrdersId(orderId)
  const pay = useMutation({ mutationFn: () => payOrder(orderId) })

  if (order.isPending) return <Skeleton className="h-96 rounded-xl" />
  if (order.isError) return <ErrorState error={order.error} onRetry={() => order.refetch()} />
  const o = order.data

  return (
    <div className="space-y-6">
      <Button variant="ghost" size="sm" asChild className="-ml-2">
        <Link to="/orders">
          <ArrowLeft />
          All orders
        </Link>
      </Button>
      <div className="flex flex-wrap items-center gap-3">
        <h1 className="text-3xl font-bold tabular">Order #{o.id!.slice(0, 8)}</h1>
        <OrderStatus status={o.status} />
      </div>
      <p className="text-muted-foreground">
        Placed {formatDateTime(o.createdAt!)}. {statusNote[o.status ?? 'New']}
      </p>

      <div className="grid gap-8 lg:grid-cols-[1fr_22rem] lg:items-start">
        <ul className="divide-y rounded-xl border bg-card">
          {o.items?.map((i) => (
            <li key={i.id} className="flex items-center justify-between gap-4 p-4">
              <div>
                <div className="font-medium">{i.productName}</div>
                <div className="text-sm text-muted-foreground tabular">
                  {i.quantity} × {formatMoney(i.unitPrice ?? 0)}
                </div>
              </div>
              <div className="font-medium tabular">{formatMoney(Number(i.unitPrice) * Number(i.quantity))}</div>
            </li>
          ))}
        </ul>
        <div className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Total</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <OrderSummary itemsTotal={Number(o.itemsTotal)} shipping={Number(o.shipping)} total={Number(o.total)} />
              {o.status === 'New' && (
                <>
                  {pay.isError && (
                    <Alert variant="destructive">
                      <AlertDescription>{pay.error.message}</AlertDescription>
                    </Alert>
                  )}
                  <Button size="lg" className="w-full" disabled={pay.isPending} onClick={() => pay.mutate()}>
                    {pay.isPending ? <Spinner /> : <CreditCard />}
                    Pay {formatMoney(o.total ?? 0)}
                  </Button>
                </>
              )}
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardTitle>Delivery</CardTitle>
            </CardHeader>
            <CardContent>
              <address className="text-sm not-italic">
                {o.customerName}
                <br />
                {o.street}
                <br />
                {o.zip} {o.city}
              </address>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  )
}
