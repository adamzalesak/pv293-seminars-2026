import { createFileRoute } from '@tanstack/react-router'
import { TicketPercent } from 'lucide-react'
import { getGetCouponsMineQueryOptions, useGetCouponsMine } from '@/api/generated/endpoints'
import { ErrorState } from '@/components/states'
import { Badge } from '@/components/ui/badge'
import { Empty, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import { Skeleton } from '@/components/ui/skeleton'
import { formatDate } from '@/lib/format'

export const Route = createFileRoute('/_shop/_customer/coupons')({
  loader: ({ context }) => context.queryClient.prefetchQuery(getGetCouponsMineQueryOptions()),
  component: MyCoupons,
})

function MyCoupons() {
  const coupons = useGetCouponsMine()

  return (
    <div className="space-y-6">
      <div className="space-y-1">
        <h1 className="text-3xl font-bold">Your referral codes</h1>
        <p className="text-muted-foreground">Every subscription you start gives you a code for a friend’s order.</p>
      </div>

      {coupons.isError ? (
        <ErrorState error={coupons.error} onRetry={() => coupons.refetch()} />
      ) : coupons.isPending ? (
        <Skeleton className="h-32 rounded-xl" />
      ) : coupons.data.length === 0 ? (
        <Empty className="border py-16">
          <EmptyHeader>
            <EmptyMedia variant="icon">
              <TicketPercent />
            </EmptyMedia>
            <EmptyTitle>No referral codes yet</EmptyTitle>
            <EmptyDescription>Start a subscription and you get a code to pass on.</EmptyDescription>
          </EmptyHeader>
        </Empty>
      ) : (
        <ul className="grid gap-4 sm:grid-cols-2">
          {coupons.data.map((c) => (
            <li key={c.code} className="space-y-2 rounded-xl border bg-card p-5">
              <div className="flex items-center justify-between gap-4">
                <span className="font-mono text-lg font-semibold">{c.code}</span>
                {c.isValid ? <Badge>{c.discountPercent} % off</Badge> : <Badge variant="secondary">Expired</Badge>}
              </div>
              <p className="text-sm text-muted-foreground">
                Valid until {formatDate(c.expiresAt)} · used {c.useCount} {Number(c.useCount) === 1 ? 'time' : 'times'}
              </p>
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}
