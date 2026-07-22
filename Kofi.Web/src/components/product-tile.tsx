import { Link } from '@tanstack/react-router'
import { Plus } from 'lucide-react'
import type { ProductDto } from '@/api/generated/model'
import { ProductImage } from '@/components/product-image'
import { RoastStamp } from '@/components/roast-stamp'
import { Button } from '@/components/ui/button'
import { toastAddedToCart } from '@/lib/added-to-cart'
import { useCart } from '@/lib/cart'
import { formatMoney } from '@/lib/format'

export function ProductTile({ product }: { product: ProductDto }) {
  const cart = useCart()
  const stock = Number(product.stockQuantity ?? 0)
  const price = Number(product.price ?? 0)
  const name = product.name ?? ''

  return (
    <article className="group relative flex flex-col overflow-hidden rounded-xl border bg-card">
      <div className="relative">
        <ProductImage
          name={name}
          imageUrl={product.imageUrl}
          className="aspect-[4/3]"
          transitionName={`product-${product.id}`}
        />
        {product.origin && (
          <span className="absolute bottom-3 left-3 rounded-md bg-background/90 px-2 py-0.5 text-xs font-medium text-foreground backdrop-blur">
            {product.origin}
          </span>
        )}
        {product.roastedOn && (
          <RoastStamp
            roastedOn={product.roastedOn}
            className="absolute top-3 right-3 shadow-sm"
            transitionName={`stamp-${product.id}`}
          />
        )}
      </div>
      <div className="flex flex-1 flex-col gap-2 p-5">
        <h3 className="text-lg leading-snug font-semibold">
          <Link
            to="/products/$productId"
            params={{ productId: product.id! }}
            viewTransition
            className="after:absolute after:inset-0 focus-visible:outline-none"
          >
            {name}
          </Link>
        </h3>
        <p className="line-clamp-2 text-sm text-muted-foreground">{product.description}</p>
        <div className="mt-auto flex items-center justify-between pt-3">
          <div>
            <div className="text-lg font-semibold tabular">{formatMoney(price)}</div>
            {stock > 0 && stock <= 15 && <div className="text-xs text-muted-foreground">Only {stock} left</div>}
            {stock === 0 && <div className="text-xs text-muted-foreground">Out of stock</div>}
          </div>
          <Button
            size="icon-lg"
            className="relative z-10"
            aria-label={`Add ${name} to cart`}
            disabled={stock === 0}
            onClick={() => {
              cart.add({ productId: product.id!, name, imageUrl: product.imageUrl, price, stock })
              toastAddedToCart(name, () => cart.setOpen(true))
            }}
          >
            <Plus />
          </Button>
        </div>
      </div>
    </article>
  )
}
