import { formatMoney } from '@/lib/format'
import { cn } from '@/lib/utils'

export function OrderSummary({
  itemsTotal,
  discount = 0,
  couponCode,
  shipping,
  total,
  className,
}: {
  itemsTotal: number
  discount?: number
  couponCode?: string | null
  shipping: number
  total: number
  className?: string
}) {
  return (
    <dl className={cn('space-y-2 text-sm', className)}>
      <div className="flex justify-between">
        <dt className="text-muted-foreground">Items</dt>
        <dd className="tabular">{formatMoney(itemsTotal)}</dd>
      </div>
      {discount > 0 && (
        <div className="flex justify-between">
          <dt className="text-muted-foreground">Coupon {couponCode}</dt>
          <dd className="tabular">−{formatMoney(discount)}</dd>
        </div>
      )}
      <div className="flex justify-between">
        <dt className="text-muted-foreground">Delivery</dt>
        <dd className="tabular">{shipping === 0 ? 'Free' : formatMoney(shipping)}</dd>
      </div>
      <div className="flex justify-between border-t pt-3 text-base font-semibold">
        <dt>Total</dt>
        <dd className="tabular">{formatMoney(total)}</dd>
      </div>
    </dl>
  )
}
