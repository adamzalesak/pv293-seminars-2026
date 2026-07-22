import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createFileRoute, Link } from '@tanstack/react-router'
import { CircleCheck, CircleX, Clock } from 'lucide-react'
import { useEffect } from 'react'
import { z } from 'zod'
import {
  getGetOrdersIdPaymentReturnQueryOptions,
  getGetOrdersIdQueryKey,
  getGetOrdersQueryKey,
  useGetOrdersIdPaymentReturn,
} from '@/api/generated/endpoints'
import { ErrorState } from '@/components/states'
import { Button } from '@/components/ui/button'
import { Empty, EmptyContent, EmptyDescription, EmptyHeader, EmptyMedia, EmptyTitle } from '@/components/ui/empty'
import { Spinner } from '@/components/ui/spinner'
import { payOrder } from '@/lib/pay'

// The provider sends the browser here. The shop then asks the provider what
// really happened and, if it was paid, marks the order paid.
export const Route = createFileRoute('/_shop/_customer/orders/$orderId/payment-return')({
  validateSearch: z.object({ paymentId: z.string() }),
  loaderDeps: ({ search }) => search,
  loader: ({ context, params, deps }) =>
    context.queryClient.prefetchQuery(getGetOrdersIdPaymentReturnQueryOptions(params.orderId, deps, confirmOnce)),
  component: PaymentReturn,
})

// Confirming marks the order paid, so it is asked once per return, not again on focus.
const confirmOnce = { query: { staleTime: Infinity, retry: false } }

function PaymentReturn() {
  const { orderId } = Route.useParams()
  const { paymentId } = Route.useSearch()
  const queryClient = useQueryClient()
  const result = useGetOrdersIdPaymentReturn(orderId, { paymentId }, confirmOnce)
  const retry = useMutation({ mutationFn: () => payOrder(orderId) })

  useEffect(() => {
    if (!result.data) return
    queryClient.invalidateQueries({ queryKey: getGetOrdersQueryKey() })
    queryClient.invalidateQueries({ queryKey: getGetOrdersIdQueryKey(orderId) })
  }, [result.data, orderId, queryClient])

  if (result.isPending) {
    return (
      <Empty className="py-20">
        <EmptyHeader>
          <EmptyMedia>
            <Spinner className="size-6" />
          </EmptyMedia>
          <EmptyTitle>Checking your payment</EmptyTitle>
        </EmptyHeader>
      </Empty>
    )
  }
  if (result.isError) return <ErrorState error={result.error} />

  const status = result.data.status
  const view =
    status === 'Paid'
      ? { icon: <CircleCheck className="text-fresh" />, title: 'Payment received', text: 'Thank you. Your order is paid and we’re packing it.' }
      : status === 'Declined'
        ? { icon: <CircleX className="text-destructive" />, title: 'Payment declined', text: 'Nothing was charged. You can try again.' }
        : status === 'Cancelled'
          ? {
              icon: <CircleX className="text-muted-foreground" />,
              title: 'Order cancelled',
              text: 'This order was cancelled, so nothing was charged. If you paid, the money is on its way back.',
            }
          : { icon: <Clock />, title: 'Payment not finished', text: 'The payment wasn’t completed. You can pick up where you left off.' }

  return (
    <Empty className="border py-16">
      <EmptyHeader>
        <EmptyMedia className="[&_svg]:size-12">{view.icon}</EmptyMedia>
        <EmptyTitle className="text-2xl">{view.title}</EmptyTitle>
        <EmptyDescription>{view.text}</EmptyDescription>
      </EmptyHeader>
      <EmptyContent className="flex-row justify-center">
        {(status === 'Declined' || status === 'Pending') && (
          <Button disabled={retry.isPending} onClick={() => retry.mutate()}>
            {retry.isPending && <Spinner />}
            Try paying again
          </Button>
        )}
        <Button variant={status === 'Paid' ? 'default' : 'outline'} asChild>
          <Link to="/orders/$orderId" params={{ orderId }}>
            View order
          </Link>
        </Button>
      </EmptyContent>
    </Empty>
  )
}
