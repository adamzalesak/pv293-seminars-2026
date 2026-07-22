import { formatMoney } from '@/lib/format'
import { cn } from '@/lib/utils'

export function OrderSummary({
  itemsTotal,
  shipping,
  total,
  className,
}: {
  itemsTotal: number
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
