import { Bean, Coffee, CookingPot, Funnel, Leaf, Settings2, Utensils } from 'lucide-react'
import { cn } from '@/lib/utils'

// A product without a photo gets the mark of what it is.
function glyphFor(name: string) {
  const n = name.toLowerCase()
  if (n.includes('kapsle')) return Coffee
  if (n.includes('čaj')) return Leaf
  if (n.includes('mlýnek')) return Settings2
  if (n.includes('v60') || n.includes('překapávač')) return Funnel
  if (n.includes('konvice')) return CookingPot
  if (n.includes('lžíce')) return Utensils
  return Bean
}

export function ProductGlyph({ name, className }: { name: string; className?: string }) {
  const Icon = glyphFor(name)
  return <Icon aria-hidden strokeWidth={1.25} className={cn('size-12', className)} />
}
