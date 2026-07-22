import { createFileRoute, Link, Outlet, useMatchRoute, useNavigate } from '@tanstack/react-router'
import { LayoutDashboard, LogOut, Package, Store, Users } from 'lucide-react'
import { AccessDenied } from '@/components/states'
import { ThemeToggle } from '@/components/theme-toggle'
import { Wordmark } from '@/components/wordmark'
import { Button } from '@/components/ui/button'
import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarHeader,
  SidebarInset,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarProvider,
  SidebarTrigger,
} from '@/components/ui/sidebar'
import { Separator } from '@/components/ui/separator'
import { requireUser } from '@/lib/guards'
import { session, useUser } from '@/lib/session'

export const Route = createFileRoute('/admin')({
  beforeLoad: ({ location }) => requireUser(location),
  component: AdminLayout,
})

const nav = [
  { to: '/admin', label: 'Overview', icon: LayoutDashboard, exact: true },
  { to: '/admin/orders', label: 'Orders', icon: Package, exact: false },
  { to: '/admin/products', label: 'Products', icon: Store, exact: false },
  { to: '/admin/customers', label: 'Customers', icon: Users, exact: false },
] as const

function AdminLayout() {
  const user = useUser()
  const navigate = useNavigate()
  const matchRoute = useMatchRoute()

  if (user && user.role !== 'Administrator') {
    return (
      <div className="mx-auto max-w-lg px-4 py-20">
        <AccessDenied>
          <Button asChild variant="outline">
            <Link to="/" search={{}}>Back to the shop</Link>
          </Button>
        </AccessDenied>
      </div>
    )
  }

  return (
    <SidebarProvider>
      <Sidebar>
        <SidebarHeader className="px-4 py-4">
          <Link to="/admin" className="flex items-baseline gap-2">
            <Wordmark />
            <span className="text-sm text-muted-foreground">Admin</span>
          </Link>
        </SidebarHeader>
        <SidebarContent>
          <SidebarGroup>
            <SidebarGroupContent>
              <SidebarMenu>
                {nav.map((item) => (
                  <SidebarMenuItem key={item.to}>
                    <SidebarMenuButton asChild isActive={!!matchRoute({ to: item.to, fuzzy: !item.exact })}>
                      <Link to={item.to}>
                        <item.icon />
                        {item.label}
                      </Link>
                    </SidebarMenuButton>
                  </SidebarMenuItem>
                ))}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        </SidebarContent>
        <SidebarFooter>
          <SidebarMenu>
            <SidebarMenuItem>
              <SidebarMenuButton asChild>
                <Link to="/" search={{}}>
                  <Store />
                  View shop
                </Link>
              </SidebarMenuButton>
            </SidebarMenuItem>
            <SidebarMenuItem>
              <SidebarMenuButton
                onClick={() => {
                  session.signOut()
                  navigate({ to: '/login', search: { redirect: undefined } })
                }}
              >
                <LogOut />
                Sign out
              </SidebarMenuButton>
            </SidebarMenuItem>
          </SidebarMenu>
          <div className="flex items-center justify-between gap-2 pl-2">
            <p className="truncate text-xs text-muted-foreground">{user?.email}</p>
            <ThemeToggle align="start" />
          </div>
        </SidebarFooter>
      </Sidebar>
      <SidebarInset>
        <header className="flex h-14 items-center gap-2 border-b px-4 md:hidden">
          <SidebarTrigger />
          <Separator orientation="vertical" className="h-4" />
          <Wordmark className="text-xl" />
        </header>
        <div className="mx-auto w-full max-w-6xl flex-1 p-4 sm:p-8">
          <Outlet />
        </div>
      </SidebarInset>
    </SidebarProvider>
  )
}
