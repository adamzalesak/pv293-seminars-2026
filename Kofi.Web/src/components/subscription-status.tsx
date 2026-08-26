import { Badge } from '@/components/ui/badge'

const subscriptionVariants = {
  Incomplete: 'outline',
  Active: 'default',
  Paused: 'secondary',
  Cancelled: 'destructive',
} as const

const subscriptionLabels: Record<string, string> = {
  Incomplete: 'Awaiting card',
  Active: 'Active',
  Paused: 'Paused',
  Cancelled: 'Cancelled',
}

export function SubscriptionStatus({ status }: { status: string }) {
  return (
    <Badge variant={subscriptionVariants[status as keyof typeof subscriptionVariants] ?? 'outline'}>
      {subscriptionLabels[status] ?? status}
    </Badge>
  )
}

const deliveryVariants = {
  Scheduled: 'outline',
  Ready: 'secondary',
  Skipped: 'outline',
  Cancelled: 'destructive',
} as const

const deliveryLabels: Record<string, string> = {
  Scheduled: 'Planned',
  Ready: 'Order placed',
  Skipped: 'Skipped',
  Cancelled: 'Cancelled',
}

// Once a delivery has its order, the order tells how the parcel is doing.
const orderLabels: Record<string, string> = {
  Shipped: 'Shipped',
  Cancelled: 'Order cancelled',
}

export function DeliveryStatus({ status, orderStatus }: { status: string; orderStatus?: string | null }) {
  const shipped = status === 'Ready' && orderStatus === 'Shipped'
  return (
    <Badge variant={shipped ? 'default' : (deliveryVariants[status as keyof typeof deliveryVariants] ?? 'outline')}>
      {(status === 'Ready' && orderStatus && orderLabels[orderStatus]) || deliveryLabels[status] || status}
    </Badge>
  )
}
