import { cn } from '@/lib/utils'

export function Wordmark({ className }: { className?: string }) {
  return (
    <span
      className={cn('font-heading text-2xl font-extrabold tracking-tight', className)}
      style={{ fontVariationSettings: '"wdth" 75' }}
    >
      KoFI
    </span>
  )
}
