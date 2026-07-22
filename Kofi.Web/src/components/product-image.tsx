import { ProductGlyph } from '@/components/product-glyph'
import { cn } from '@/lib/utils'

// The product's photo from the API when it has one, its glyph on a muted
// ground otherwise. Images are served by the API, so they go through /api.
export function ProductImage({
  name,
  imageUrl,
  className,
  glyphClassName,
  eager,
  transitionName,
}: {
  name: string
  imageUrl?: string | null
  className?: string
  glyphClassName?: string
  eager?: boolean
  // Shared with the same product's image on the next page, so the browser
  // morphs one into the other (View Transitions).
  transitionName?: string
}) {
  return (
    <div
      className={cn('relative overflow-hidden bg-muted text-muted-foreground', className)}
      style={transitionName ? { viewTransitionName: transitionName } : undefined}
    >
      {imageUrl ? (
        <img
          src={`/api${imageUrl}`}
          alt=""
          loading={eager ? 'eager' : 'lazy'}
          decoding="async"
          className="absolute inset-0 size-full object-cover"
        />
      ) : (
        <div className="absolute inset-0 flex items-end p-[8%]">
          <ProductGlyph name={name} className={glyphClassName} />
        </div>
      )}
    </div>
  )
}
