import { Link } from '@tanstack/react-router'
import type { ReactNode } from 'react'
import { useGetProductsId } from '@/api/generated/endpoints'
import type { SubscriptionDeliveryDetail, SubscriptionDetail, SubscriptionExtraDetail } from '@/api/generated/model'
import { DeliveryStatus } from '@/components/subscription-status'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { formatDate, formatMoney } from '@/lib/format'
import { deliveryTotal } from '@/lib/subscriptions'

// A delivery keeps the coffee it was planned with; after a plan change that is
// no longer the subscription's product, so its name is looked up.
function BaseItemName({ subscription, productId }: { subscription: SubscriptionDetail; productId: string }) {
  const same = productId === subscription.productId
  const product = useGetProductsId(productId, { query: { enabled: !same } })
  return <>{same ? subscription.productName : (product.data?.name ?? '…')}</>
}

export function SubscriptionDeliveries({
  subscription,
  orderLink,
  actions,
  extraActions,
}: {
  subscription: SubscriptionDetail
  orderLink: '/orders/$orderId' | '/admin/orders/$orderId'
  actions?: (delivery: SubscriptionDeliveryDetail) => ReactNode
  extraActions?: (delivery: SubscriptionDeliveryDetail, extra: SubscriptionExtraDetail) => ReactNode
}) {
  // Newest first: the planned cycle on top, history below.
  const deliveries = [...subscription.deliveries].sort((a, b) => b.scheduledFor.localeCompare(a.scheduledFor))

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead>Date</TableHead>
          <TableHead>Contents</TableHead>
          <TableHead>Status</TableHead>
          <TableHead className="text-right">Total</TableHead>
          {actions && <TableHead className="text-right" />}
        </TableRow>
      </TableHeader>
      <TableBody>
        {deliveries.map((d) => (
          <TableRow key={d.id} className="align-top">
            <TableCell className="tabular">{formatDate(d.scheduledFor)}</TableCell>
            <TableCell className="whitespace-normal">
              <div>
                {d.quantity} × <BaseItemName subscription={subscription} productId={d.productId} />
              </div>
              {d.extras.map((e) => (
                <div key={e.productId} className="flex items-center gap-1 text-sm text-muted-foreground">
                  <span>
                    + {e.quantity} × {e.productName}
                  </span>
                  {extraActions?.(d, e)}
                </div>
              ))}
              {d.orderId && (
                <Link
                  to={orderLink}
                  params={{ orderId: d.orderId }}
                  className="text-sm text-muted-foreground underline underline-offset-4 hover:text-foreground"
                >
                  Order #{d.orderId.slice(0, 8)}
                </Link>
              )}
            </TableCell>
            <TableCell>
              <DeliveryStatus status={d.status} orderStatus={d.orderStatus} />
            </TableCell>
            <TableCell className="text-right tabular">{formatMoney(deliveryTotal(d))}</TableCell>
            {actions && <TableCell className="text-right">{actions(d)}</TableCell>}
          </TableRow>
        ))}
        {deliveries.length === 0 && (
          <TableRow>
            <TableCell colSpan={actions ? 5 : 4} className="py-8 text-center text-muted-foreground">
              The first delivery is planned once your card is authorised.
            </TableCell>
          </TableRow>
        )}
      </TableBody>
    </Table>
  )
}
