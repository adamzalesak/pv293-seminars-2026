import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { intervalLabel, intervals } from '@/lib/subscriptions'

export function IntervalSelect({ value, onChange }: { value: number; onChange: (days: number) => void }) {
  // A plan made elsewhere may use an interval the shop does not offer; keep it selectable.
  const options = intervals.some((i) => i.days === value) ? intervals : [...intervals, { days: value, label: intervalLabel(value) }]
  return (
    <Select value={String(value)} onValueChange={(v) => onChange(Number(v))}>
      <SelectTrigger className="w-full" aria-label="Delivery interval">
        <SelectValue />
      </SelectTrigger>
      <SelectContent>
        {options.map((i) => (
          <SelectItem key={i.days} value={String(i.days)}>
            {i.label}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  )
}
