import { createFileRoute, Link, Outlet } from '@tanstack/react-router'
import { AccessDenied } from '@/components/states'
import { Button } from '@/components/ui/button'
import { requireUser } from '@/lib/guards'
import { useUser } from '@/lib/session'

// Placing, listing and paying orders is the customer's; the administrator
// works with orders in /admin.
export const Route = createFileRoute('/_shop/_customer')({
  beforeLoad: ({ location }) => requireUser(location),
  component: CustomerArea,
})

function CustomerArea() {
  const user = useUser()
  if (user && user.role !== 'Customer') {
    return (
      <AccessDenied>
        <Button asChild variant="outline">
          <Link to="/admin">Go to admin</Link>
        </Button>
      </AccessDenied>
    )
  }
  return <Outlet />
}
