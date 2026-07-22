import { useNavigate } from '@tanstack/react-router'
import { ShoppingBag } from 'lucide-react'
import { useEffect } from 'react'
import { toast } from 'sonner'
import { CartLines } from '@/components/cart-lines'
import { OrderSummary } from '@/components/order-summary'
import { Button } from '@/components/ui/button'
import { Empty, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import { Sheet, SheetContent, SheetDescription, SheetFooter, SheetHeader, SheetTitle } from '@/components/ui/sheet'
import { FREE_SHIPPING_FROM, shippingFor, useCart } from '@/lib/cart'
import { formatMoney } from '@/lib/format'

export function CartSheet() {
  const cart = useCart()
  const navigate = useNavigate()
  const shipping = shippingFor(cart.itemsTotal)
  const toFree = FREE_SHIPPING_FROM - cart.itemsTotal
  const close = () => cart.setOpen(false)

  // The drawer shows what the "Added to cart" toasts were about.
  useEffect(() => {
    if (cart.open) toast.dismiss()
  }, [cart.open])

  return (
    <Sheet open={cart.open} onOpenChange={cart.setOpen}>
      <SheetContent className="gap-0 p-0 data-[side=right]:w-full data-[side=right]:sm:max-w-md">
        <SheetHeader className="border-b p-5">
          <SheetTitle className="font-heading text-xl">Your cart</SheetTitle>
          <SheetDescription>
            {cart.count === 0 ? 'Nothing in it yet.' : `${cart.count} ${cart.count === 1 ? 'item' : 'items'}`}
          </SheetDescription>
        </SheetHeader>

        {cart.lines.length === 0 ? (
          <Empty className="flex-1">
            <EmptyHeader>
              <EmptyMedia variant="icon">
                <ShoppingBag />
              </EmptyMedia>
              <EmptyTitle>Your cart is empty</EmptyTitle>
              <EmptyDescription>Add something from the shop and it shows up here.</EmptyDescription>
            </EmptyHeader>
          </Empty>
        ) : (
          <>
            <CartLines className="flex-1 overflow-y-auto" onNavigate={close} />
            <SheetFooter className="gap-4 border-t p-5">
              {toFree > 0 && (
                <p className="rounded-lg bg-secondary px-3 py-2 text-sm text-secondary-foreground">
                  Add {formatMoney(toFree)} more for free delivery.
                </p>
              )}
              <OrderSummary itemsTotal={cart.itemsTotal} shipping={shipping} total={cart.itemsTotal + shipping} />
              <Button
                size="lg"
                className="w-full"
                onClick={() => {
                  close()
                  navigate({ to: '/checkout' })
                }}
              >
                Go to checkout
              </Button>
            </SheetFooter>
          </>
        )}
      </SheetContent>
    </Sheet>
  )
}
