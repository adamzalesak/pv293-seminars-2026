import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createFileRoute, Link } from '@tanstack/react-router'
import { CircleCheck, CircleX, Clock } from 'lucide-react'
import { useEffect } from 'react'
import { z } from 'zod'
import {
  getGetSubscriptionsIdCardReturnQueryOptions,
  getGetSubscriptionsIdQueryKey,
  getGetSubscriptionsQueryKey,
  useGetSubscriptionsIdCardReturn,
} from '@/api/generated/endpoints'
import { ErrorState } from '@/components/states'
import { Button } from '@/components/ui/button'
import { Empty, EmptyContent, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import { Spinner } from '@/components/ui/spinner'
import { authoriseCard } from '@/lib/authorise-card'

// The provider sends the browser here. The shop then asks the provider whether
// the card was authorised and, if it was, activates the subscription.
export const Route = createFileRoute('/_shop/_customer/subscriptions/$subscriptionId/card-return')({
  validateSearch: z.object({ setupId: z.string() }),
  loaderDeps: ({ search }) => search,
  loader: ({ context, params, deps }) =>
    context.queryClient.prefetchQuery(getGetSubscriptionsIdCardReturnQueryOptions(params.subscriptionId, deps, confirmOnce)),
  component: CardReturn,
})

// Activating may prepare and charge the first delivery, so it is asked once per return.
const confirmOnce = { query: { staleTime: Infinity, retry: false } }

function CardReturn() {
  const { subscriptionId } = Route.useParams()
  const { setupId } = Route.useSearch()
  const queryClient = useQueryClient()
  const result = useGetSubscriptionsIdCardReturn(subscriptionId, { setupId }, confirmOnce)
  const retry = useMutation({ mutationFn: () => authoriseCard(subscriptionId) })

  useEffect(() => {
    if (!result.data) return
    queryClient.invalidateQueries({ queryKey: getGetSubscriptionsQueryKey() })
    queryClient.invalidateQueries({ queryKey: getGetSubscriptionsIdQueryKey(subscriptionId) })
  }, [result.data, subscriptionId, queryClient])

  if (result.isPending) {
    return (
      <Empty className="py-20">
        <EmptyHeader>
          <EmptyMedia>
            <Spinner className="size-6" />
          </EmptyMedia>
          <EmptyTitle>Checking your card</EmptyTitle>
        </EmptyHeader>
      </Empty>
    )
  }
  if (result.isError) return <ErrorState error={result.error} />

  const { status, cardSetupStatus } = result.data
  const view =
    status === 'Active' || status === 'Paused'
      ? { icon: <CircleCheck className="text-fresh" />, title: 'Subscription active', text: 'Your card is saved. Each delivery is charged to it when we prepare it.' }
      : cardSetupStatus === 'Declined'
        ? { icon: <CircleX className="text-destructive" />, title: 'Card not authorised', text: 'Nothing was charged. You can try again.' }
        : { icon: <Clock />, title: 'Card not authorised yet', text: 'The authorisation wasn’t finished. You can pick up where you left off.' }

  return (
    <Empty className="border py-16">
      <EmptyHeader>
        <EmptyMedia className="[&_svg]:size-12">{view.icon}</EmptyMedia>
        <EmptyTitle className="text-2xl">{view.title}</EmptyTitle>
        <EmptyDescription>{view.text}</EmptyDescription>
      </EmptyHeader>
      <EmptyContent className="flex-row justify-center">
        {status === 'Incomplete' && (
          <Button disabled={retry.isPending} onClick={() => retry.mutate()}>
            {retry.isPending && <Spinner />}
            Authorise a card
          </Button>
        )}
        <Button variant={status === 'Active' ? 'default' : 'outline'} asChild>
          <Link to="/subscriptions/$subscriptionId" params={{ subscriptionId }}>
            View subscription
          </Link>
        </Button>
      </EmptyContent>
    </Empty>
  )
}
