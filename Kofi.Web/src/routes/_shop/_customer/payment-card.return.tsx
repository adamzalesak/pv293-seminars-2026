import { useQueryClient } from '@tanstack/react-query'
import { createFileRoute, Link } from '@tanstack/react-router'
import { CircleCheck, CircleX, Clock } from 'lucide-react'
import { useEffect } from 'react'
import { z } from 'zod'
import {
  getGetPaymentCardQueryKey,
  getGetPaymentCardReturnQueryOptions,
  useGetPaymentCardReturn,
} from '@/api/generated/endpoints'
import { ErrorState } from '@/components/states'
import { Button } from '@/components/ui/button'
import { Empty, EmptyContent, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import { Spinner } from '@/components/ui/spinner'

// The provider sends the browser here after a card change. The shop asks the
// provider what was authorised before it replaces the saved card.
export const Route = createFileRoute('/_shop/_customer/payment-card/return')({
  validateSearch: z.object({ setupId: z.string() }),
  loaderDeps: ({ search }) => search,
  loader: ({ context, deps }) =>
    context.queryClient.prefetchQuery(getGetPaymentCardReturnQueryOptions(deps, confirmOnce)),
  component: PaymentCardReturn,
})

const confirmOnce = { query: { staleTime: Infinity, retry: false } }

function PaymentCardReturn() {
  const { setupId } = Route.useSearch()
  const queryClient = useQueryClient()
  const result = useGetPaymentCardReturn({ setupId }, confirmOnce)

  useEffect(() => {
    if (result.data) queryClient.invalidateQueries({ queryKey: getGetPaymentCardQueryKey() })
  }, [result.data, queryClient])

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

  const { cardSetupStatus, label } = result.data
  const view =
    cardSetupStatus === 'Succeeded'
      ? { icon: <CircleCheck className="text-fresh" />, title: 'Card changed', text: `Every delivery is now charged to ${label}.` }
      : cardSetupStatus === 'Declined'
        ? { icon: <CircleX className="text-destructive" />, title: 'Card not changed', text: 'Your deliveries are still charged to your previous card.' }
        : { icon: <Clock />, title: 'Card not changed yet', text: 'The authorisation wasn’t finished. Your previous card stays in place.' }

  return (
    <Empty className="border py-16">
      <EmptyHeader>
        <EmptyMedia className="[&_svg]:size-12">{view.icon}</EmptyMedia>
        <EmptyTitle className="text-2xl">{view.title}</EmptyTitle>
        <EmptyDescription>{view.text}</EmptyDescription>
      </EmptyHeader>
      <EmptyContent className="flex-row justify-center">
        <Button asChild>
          <Link to="/subscriptions">Your subscriptions</Link>
        </Button>
      </EmptyContent>
    </Empty>
  )
}
