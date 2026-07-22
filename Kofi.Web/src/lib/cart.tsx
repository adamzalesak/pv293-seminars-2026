import { createContext, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'

// The cart lives only in the browser; the API sees it once, as the order.
export type CartLine = {
  productId: string
  name: string
  imageUrl?: string | null
  price: number
  quantity: number
  stock: number
}

type Cart = {
  lines: CartLine[]
  count: number
  itemsTotal: number
  add: (line: Omit<CartLine, 'quantity'>, quantity?: number) => void
  setQuantity: (productId: string, quantity: number) => void
  remove: (productId: string) => void
  clear: () => void
  // The cart drawer; UI state only, not remembered.
  open: boolean
  setOpen: (open: boolean) => void
}

const KEY = 'kofi.cart'
const CartContext = createContext<Cart | null>(null)

function load(): CartLine[] {
  try {
    return JSON.parse(localStorage.getItem(KEY) ?? '[]')
  } catch {
    return []
  }
}

export function CartProvider({ children }: { children: ReactNode }) {
  const [lines, setLines] = useState<CartLine[]>(load)
  const [open, setOpen] = useState(false)

  useEffect(() => {
    try {
      localStorage.setItem(KEY, JSON.stringify(lines))
    } catch {
      // Storage unavailable: the cart lasts as long as the tab.
    }
  }, [lines])

  const cart = useMemo<Cart>(() => {
    const clamp = (line: CartLine, quantity: number) =>
      Math.max(1, Math.min(quantity, line.stock))
    return {
      lines,
      count: lines.reduce((sum, l) => sum + l.quantity, 0),
      itemsTotal: lines.reduce((sum, l) => sum + l.price * l.quantity, 0),
      add: (line, quantity = 1) =>
        setLines((current) => {
          const existing = current.find((l) => l.productId === line.productId)
          if (!existing) return [...current, { ...line, quantity: Math.min(quantity, line.stock) }]
          return current.map((l) =>
            l.productId === line.productId
              ? { ...l, ...line, quantity: clamp({ ...l, ...line }, l.quantity + quantity) }
              : l,
          )
        }),
      setQuantity: (productId, quantity) =>
        setLines((current) =>
          current.map((l) => (l.productId === productId ? { ...l, quantity: clamp(l, quantity) } : l)),
        ),
      remove: (productId) => setLines((current) => current.filter((l) => l.productId !== productId)),
      clear: () => setLines([]),
      open,
      setOpen,
    }
  }, [lines, open])

  return <CartContext.Provider value={cart}>{children}</CartContext.Provider>
}

export function useCart() {
  const cart = useContext(CartContext)
  if (!cart) throw new Error('useCart outside CartProvider')
  return cart
}

// Mirrors the shop's rule, so the cart can show it before the order exists.
export const FREE_SHIPPING_FROM = 50
export const SHIPPING = 4.99
export const shippingFor = (itemsTotal: number) => (itemsTotal < FREE_SHIPPING_FROM ? SHIPPING : 0)
