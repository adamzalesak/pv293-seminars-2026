import { Link } from '@tanstack/react-router'
import { Minus, Plus, Trash2 } from 'lucide-react'
import { ProductImage } from '@/components/product-image'
import { Button } from '@/components/ui/button'
import { useCart } from '@/lib/cart'
import { formatMoney } from '@/lib/format'
import { cn } from '@/lib/utils'

// The cart's lines with quantity controls; shared by the cart page and drawer.
export function CartLines({ className, onNavigate }: { className?: string; onNavigate?: () => void }) {
  const cart = useCart()

  return (
    <ul className={cn('divide-y', className)}>
      {cart.lines.map((line) => (
        <li key={line.productId} className="flex gap-4 p-4">
          <ProductImage
            name={line.name}
            imageUrl={line.imageUrl}
            className="size-16 shrink-0 rounded-lg"
            glyphClassName="size-7"
          />
          <div className="flex min-w-0 flex-1 flex-col gap-2">
            <div className="flex items-start justify-between gap-3">
              <div className="min-w-0">
                <Link
                  to="/products/$productId"
                  params={{ productId: line.productId }}
                  className="font-medium hover:underline"
                  onClick={onNavigate}
                >
                  {line.name}
                </Link>
                <div className="text-sm text-muted-foreground tabular">{formatMoney(line.price)} each</div>
              </div>
              <div className="font-medium tabular">{formatMoney(line.price * line.quantity)}</div>
            </div>
            <div className="flex items-center justify-between">
              <div className="flex items-center rounded-lg border">
                <Button
                  variant="ghost"
                  size="icon-sm"
                  aria-label={`Fewer ${line.name}`}
                  disabled={line.quantity <= 1}
                  onClick={() => cart.setQuantity(line.productId, line.quantity - 1)}
                >
                  <Minus />
                </Button>
                <span className="w-8 text-center text-sm tabular" aria-label="Quantity">
                  {line.quantity}
                </span>
                <Button
                  variant="ghost"
                  size="icon-sm"
                  aria-label={`More ${line.name}`}
                  disabled={line.quantity >= line.stock}
                  onClick={() => cart.setQuantity(line.productId, line.quantity + 1)}
                >
                  <Plus />
                </Button>
              </div>
              <Button
                variant="ghost"
                size="sm"
                className="text-muted-foreground"
                onClick={() => cart.remove(line.productId)}
              >
                <Trash2 />
                Remove
              </Button>
            </div>
          </div>
        </li>
      ))}
    </ul>
  )
}
