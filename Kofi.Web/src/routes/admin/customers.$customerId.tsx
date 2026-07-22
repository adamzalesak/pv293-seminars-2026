import { createFileRoute, Link } from '@tanstack/react-router'
import { ArrowLeft, Star } from 'lucide-react'
import { getGetCustomersIdQueryOptions, useGetCustomersId, usePostCustomersIdVip } from '@/api/generated/endpoints'
import { PageTitle } from '@/components/page-title'
import { ErrorState } from '@/components/states'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Skeleton } from '@/components/ui/skeleton'
import { formatDate } from '@/lib/format'
import { useOnDone } from '@/lib/use-on-done'

export const Route = createFileRoute('/admin/customers/$customerId')({
  loader: ({ context, params }) => context.queryClient.prefetchQuery(getGetCustomersIdQueryOptions(params.customerId)),
  component: AdminCustomer,
})

function AdminCustomer() {
  const { customerId } = Route.useParams()
  const customer = useGetCustomersId(customerId)
  const onDone = useOnDone()
  const vip = usePostCustomersIdVip({ mutation: onDone('Customer is now VIP', '/customers') })

  if (customer.isPending) return <Skeleton className="h-64 rounded-xl" />
  if (customer.isError) return <ErrorState error={customer.error} onRetry={() => customer.refetch()} />
  const c = customer.data

  return (
    <>
      <Button variant="ghost" size="sm" asChild className="mb-4 -ml-2">
        <Link to="/admin/customers">
          <ArrowLeft />
          Customers
        </Link>
      </Button>
      <PageTitle
        title={
          <span className="flex items-center gap-3">
            {c.name}
            {c.isVip && <Badge variant="secondary">VIP</Badge>}
          </span>
        }
        description={c.email}
        actions={
          !c.isVip && (
            <Button disabled={vip.isPending} onClick={() => vip.mutate({ id: customerId })}>
              <Star />
              Make VIP
            </Button>
          )
        }
      />
      <Card className="max-w-lg">
        <CardContent>
          <dl className="grid grid-cols-[8rem_1fr] gap-y-3 text-sm">
            <dt className="text-muted-foreground">Address</dt>
            <dd>
              {c.street}
              <br />
              {c.zip} {c.city}
            </dd>
            <dt className="text-muted-foreground">Customer since</dt>
            <dd>{formatDate(c.registeredAt!)}</dd>
          </dl>
        </CardContent>
      </Card>
    </>
  )
}
