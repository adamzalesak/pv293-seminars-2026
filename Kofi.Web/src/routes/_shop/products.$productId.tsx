import { createFileRoute, Link, useCanGoBack, useRouter } from '@tanstack/react-router'
import { ArrowLeft, Minus, Plus } from 'lucide-react'
import { useState } from 'react'
import { getGetProductsIdQueryOptions, useGetProductsId } from '@/api/generated/endpoints'
import { ProductImage } from '@/components/product-image'
import { RoastStamp } from '@/components/roast-stamp'
import { ErrorState } from '@/components/states'
import { Button } from '@/components/ui/button'
import { Skeleton } from '@/components/ui/skeleton'
import { toastAddedToCart } from '@/lib/added-to-cart'
import { useCart } from '@/lib/cart'
import { formatDate, formatMoney } from '@/lib/format'

export const Route = createFileRoute('/_shop/products/$productId')({
  loader: ({ context, params }) => context.queryClient.prefetchQuery(getGetProductsIdQueryOptions(params.productId)),
  component: ProductDetail,
})

function ProductDetail() {
  const { productId } = Route.useParams()
  const product = useGetProductsId(productId)
  const cart = useCart()
  const router = useRouter()
  const canGoBack = useCanGoBack()
  const [quantity, setQuantity] = useState(1)

  if (product.isPending) return <Skeleton className="h-96 rounded-xl" />
  if (product.isError) return <ErrorState error={product.error} onRetry={() => product.refetch()} />

  const p = product.data
  const stock = Number(p.stockQuantity ?? 0)
  const price = Number(p.price ?? 0)
  const name = p.name ?? ''
  const inCart = cart.lines.find((l) => l.productId === p.id)?.quantity ?? 0
  const canAdd = p.isActive && stock - inCart > 0

  return (
    <div className="space-y-6">
      <Button variant="ghost" size="sm" asChild className="-ml-2">
        <Link
          to="/"
          search={{}}
          viewTransition
          onClick={(e) => {
            // Back to where the customer was in the catalogue, scroll position,
            // page and search included. A fresh /products link has nowhere to go
            // back to, so it opens the catalogue instead.
            // Going back skips navigate(), so the view transition is asked for here.
            if (canGoBack) {
              e.preventDefault()
              router.shouldViewTransition = true
              router.history.back()
            }
          }}
        >
          <ArrowLeft />
          All products
        </Link>
      </Button>
      <div className="grid gap-10 md:grid-cols-2">
        <div className="relative">
          <ProductImage
            name={name}
            imageUrl={p.imageUrl}
            eager
            className="aspect-4/3 rounded-xl md:aspect-5/4"
            glyphClassName="size-28"
            transitionName={`product-${p.id}`}
          />
          {p.roastedOn && (
            <RoastStamp
              roastedOn={p.roastedOn}
              className="absolute top-6 right-6 size-24 shadow-sm [&>span:first-child]:text-3xl"
              transitionName={`stamp-${p.id}`}
            />
          )}
        </div>
        <div className="flex flex-col gap-6">
          <div className="space-y-3">
            <h1 className="text-4xl font-bold">{name}</h1>
            <p className="text-lg text-muted-foreground">{p.description}</p>
          </div>
          <dl className="grid grid-cols-2 gap-x-6 gap-y-3 border-y py-5 text-sm">
            {p.origin && (
              <>
                <dt className="text-muted-foreground">Origin</dt>
                <dd>{p.origin}</dd>
              </>
            )}
            {p.roastedOn && (
              <>
                <dt className="text-muted-foreground">Roasted on</dt>
                <dd>{formatDate(p.roastedOn)}</dd>
              </>
            )}
            <dt className="text-muted-foreground">In stock</dt>
            <dd className="tabular">{p.isActive ? stock : 'No longer sold'}</dd>
          </dl>
          <div className="text-3xl font-semibold tabular">{formatMoney(price)}</div>
          <div className="flex items-center gap-3">
            <div className="flex items-center rounded-lg border">
              <Button variant="ghost" size="icon" aria-label="Fewer" onClick={() => setQuantity((q) => Math.max(1, q - 1))}>
                <Minus />
              </Button>
              <span className="w-10 text-center tabular" aria-live="polite">
                {quantity}
              </span>
              <Button
                variant="ghost"
                size="icon"
                aria-label="More"
                onClick={() => setQuantity((q) => Math.min(Math.max(1, stock - inCart), q + 1))}
              >
                <Plus />
              </Button>
            </div>
            <Button
              size="lg"
              className="flex-1"
              disabled={!canAdd}
              onClick={() => {
                cart.add({ productId: p.id!, name, imageUrl: p.imageUrl, price, stock }, quantity)
                toastAddedToCart(`${quantity} × ${name}`, () => cart.setOpen(true))
                setQuantity(1)
              }}
            >
              Add to cart
            </Button>
          </div>
          {inCart > 0 && (
            <p className="text-sm text-muted-foreground">
              {inCart} already in your <Link to="/cart" className="underline underline-offset-4">cart</Link>.
            </p>
          )}
        </div>
      </div>
    </div>
  )
}
