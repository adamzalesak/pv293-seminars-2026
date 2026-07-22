import { createFileRoute, Link, Outlet, useNavigate } from '@tanstack/react-router'
import { LogOut, ShoppingBag, User } from 'lucide-react'
import { ThemeToggle } from '@/components/theme-toggle'
import { Wordmark } from '@/components/wordmark'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { useCart } from '@/lib/cart'
import { formatMoney } from '@/lib/format'
import { session, useUser } from '@/lib/session'

export const Route = createFileRoute('/_shop')({
  component: ShopLayout,
})

const navLink =
  'text-sm text-muted-foreground transition-colors hover:text-foreground data-[status=active]:text-foreground data-[status=active]:font-medium'

function ShopLayout() {
  const user = useUser()
  const cart = useCart()
  const navigate = useNavigate()

  return (
    <div className="flex min-h-svh flex-col">
      <header className="sticky top-0 z-30 border-b bg-background/90 backdrop-blur">
        <div className="mx-auto flex h-16 max-w-6xl items-center gap-6 px-4 sm:px-6">
          <Link to="/" search={{}} aria-label="KoFI home">
            <Wordmark />
          </Link>
          <nav className="flex items-center gap-5">
            <Link to="/" search={{}} activeOptions={{ exact: true, includeSearch: false }} className={navLink}>
              Shop
            </Link>
            {user?.role === 'Customer' && (
              <Link to="/orders" className={navLink}>
                Orders
              </Link>
            )}
            {user?.role === 'Administrator' && (
              <Link to="/admin" className={navLink}>
                Admin
              </Link>
            )}
          </nav>
          <div className="ml-auto flex items-center gap-2">
            <ThemeToggle />
            <Button variant="ghost" aria-label={`Cart, ${cart.count} items`} onClick={() => cart.setOpen(true)}>
              <ShoppingBag />
              {cart.count > 0 && <span className="hidden tabular sm:inline">{formatMoney(cart.itemsTotal)}</span>}
              {cart.count > 0 && (
                // Re-keyed on every change, so the count pops when something is added.
                <Badge key={cart.count} className="tabular motion-safe:animate-in motion-safe:zoom-in-50 motion-safe:duration-300">
                  {cart.count}
                </Badge>
              )}
            </Button>
            {user ? (
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="outline" size="icon" aria-label="Account">
                    <User />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="min-w-56">
                  <DropdownMenuLabel className="font-normal">
                    <div className="text-sm font-medium">{user.email}</div>
                    <div className="text-xs text-muted-foreground">{user.role}</div>
                  </DropdownMenuLabel>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem
                    onSelect={() => {
                      session.signOut()
                      navigate({ to: '/', search: {} })
                    }}
                  >
                    <LogOut />
                    Sign out
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            ) : (
              <Button variant="outline" asChild>
                <Link to="/login" search={{ redirect: undefined }}>
                  Sign in
                </Link>
              </Button>
            )}
          </div>
        </div>
      </header>
      <main className="mx-auto w-full max-w-6xl flex-1 px-4 py-8 sm:px-6 sm:py-10">
        <Outlet />
      </main>
      <footer className="border-t">
        <div className="mx-auto flex max-w-6xl flex-col gap-1 px-4 py-6 text-sm text-muted-foreground sm:flex-row sm:justify-between sm:px-6">
          <p>Coffee and brewing kit from a dozen Czech roasters.</p>
          <p>Free delivery on orders from {formatMoney(50)}.</p>
        </div>
      </footer>
    </div>
  )
}
