import { toast } from 'sonner'

// One confirmation for every "Add to cart", with a way into the drawer.
export function toastAddedToCart(label: string, openCart: () => void) {
  toast.success(`Added ${label} to cart`, {
    action: { label: 'View cart', onClick: openCart },
  })
}
