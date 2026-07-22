import { Badge } from '@/components/ui/badge'

const variants = {
  New: 'outline',
  Paid: 'secondary',
  Shipped: 'default',
  Cancelled: 'destructive',
} as const

const labels: Record<string, string> = {
  New: 'Awaiting payment',
  Paid: 'Paid',
  Shipped: 'Shipped',
  Cancelled: 'Cancelled',
}

export function OrderStatus({ status }: { status?: string }) {
  const s = status ?? 'New'
  return <Badge variant={variants[s as keyof typeof variants] ?? 'outline'}>{labels[s] ?? s}</Badge>
}
