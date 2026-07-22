import { daysSince, formatDate } from '@/lib/format'
import { cn } from '@/lib/utils'

// Beans are at their best roughly two weeks after roasting; after that the
// stamp turns from green-bean to crema.
const PEAK_DAYS = 14

export function RoastStamp({
  roastedOn,
  className,
  transitionName,
}: {
  roastedOn: string
  className?: string
  transitionName?: string
}) {
  const days = daysSince(roastedOn)
  const fresh = days <= PEAK_DAYS
  return (
    <span
      title={`Roasted on ${formatDate(roastedOn)}`}
      style={transitionName ? { viewTransitionName: transitionName } : undefined}
      className={cn(
        'inline-flex flex-col items-center justify-center rounded-full border-2 border-dashed font-heading leading-none',
        'size-16 -rotate-6 text-center',
        fresh
          ? 'border-fresh bg-fresh text-fresh-foreground'
          : 'border-resting-foreground/40 bg-resting text-resting-foreground',
        className,
      )}
    >
      <span className="text-xl font-bold tabular">{days}</span>
      <span className="text-[0.625rem] font-medium">{days === 1 ? 'day' : 'days'} off roast</span>
    </span>
  )
}
