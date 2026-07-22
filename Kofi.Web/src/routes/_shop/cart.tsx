import { createFileRoute, Link } from '@tanstack/react-router'
import { ShoppingBag } from 'lucide-react'
import { CartLines } from '@/components/cart-lines'
import { OrderSummary } from '@/components/order-summary'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from '@/components/ui/card'
import { Empty, EmptyContent, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import { FREE_SHIPPING_FROM, shippingFor, useCart } from '@/lib/cart'
import { formatMoney } from '@/lib/format'

export const Route = createFileRoute('/_shop/cart')({
  component: CartPage,
})

function CartPage() {
  const cart = useCart()

  if (cart.lines.length === 0) {
    return (
      <Empty className="border py-16">
        <EmptyHeader>
          <EmptyMedia variant="icon">
            <ShoppingBag />
          </EmptyMedia>
          <EmptyTitle>Your cart is empty</EmptyTitle>
          <EmptyDescription>Pick a bag of something roasted this week.</EmptyDescription>
        </EmptyHeader>
        <EmptyContent>
          <Button asChild>
            <Link to="/" search={{}}>Browse the shop</Link>
          </Button>
        </EmptyContent>
      </Empty>
    )
  }

  const shipping = shippingFor(cart.itemsTotal)
  const toFree = FREE_SHIPPING_FROM - cart.itemsTotal

  return (
    <div className="space-y-8">
      <h1 className="text-3xl font-bold">Cart</h1>
      <div className="grid gap-8 lg:grid-cols-[1fr_22rem] lg:items-start">
        <CartLines className="rounded-xl border bg-card" />
        <Card>
          <CardHeader>
            <CardTitle>Summary</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <OrderSummary itemsTotal={cart.itemsTotal} shipping={shipping} total={cart.itemsTotal + shipping} />
            {toFree > 0 && (
              <p className="rounded-lg bg-secondary px-3 py-2 text-sm text-secondary-foreground">
                Add {formatMoney(toFree)} more for free delivery.
              </p>
            )}
          </CardContent>
          <CardFooter>
            <Button asChild size="lg" className="w-full">
              <Link to="/checkout">Go to checkout</Link>
            </Button>
          </CardFooter>
        </Card>
      </div>
    </div>
  )
}
