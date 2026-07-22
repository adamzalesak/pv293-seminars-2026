import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createFileRoute, Link, useNavigate } from '@tanstack/react-router'
import { useForm } from 'react-hook-form'
import { toast } from 'sonner'
import { z } from 'zod'
import { postOrders } from '@/api/generated/endpoints'
import { OrderSummary } from '@/components/order-summary'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Field, FieldError, FieldGroup, FieldLabel } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { Spinner } from '@/components/ui/spinner'
import { shippingFor, useCart } from '@/lib/cart'
import { formatMoney } from '@/lib/format'
import { payOrder } from '@/lib/pay'
import { invalidate } from '@/lib/query'

export const Route = createFileRoute('/_shop/_customer/checkout')({
  component: Checkout,
})

const schema = z.object({
  customerName: z.string().trim().min(1, 'Enter the name for the delivery.'),
  street: z.string().trim().min(1, 'Enter a street and number.'),
  city: z.string().trim().min(1, 'Enter a city.'),
  zip: z.string().trim().min(3, 'Enter a postcode.'),
})
type Address = z.infer<typeof schema>

const ADDRESS_KEY = 'kofi.address'
function rememberedAddress(): Address {
  try {
    return { customerName: '', street: '', city: '', zip: '', ...JSON.parse(localStorage.getItem(ADDRESS_KEY) ?? '{}') }
  } catch {
    return { customerName: '', street: '', city: '', zip: '' }
  }
}

function Checkout() {
  const cart = useCart()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const form = useForm<Address>({ resolver: zodResolver(schema), defaultValues: rememberedAddress() })

  const place = useMutation({
    mutationFn: (address: Address) =>
      postOrders({
        ...address,
        items: cart.lines.map((l) => ({ productId: l.productId, quantity: l.quantity })),
      }),
    onSuccess: async (order, address) => {
      try {
        localStorage.setItem(ADDRESS_KEY, JSON.stringify(address))
      } catch {
        // Not remembering the address is fine.
      }
      cart.clear()
      await invalidate(queryClient, '/orders', '/products')
      try {
        await payOrder(order.id!)
      } catch {
        toast.error('Your order is placed, but the payment page didn’t open. Pay from the order.')
        navigate({ to: '/orders/$orderId', params: { orderId: order.id! } })
      }
    },
  })

  if (cart.lines.length === 0 && !place.isSuccess) {
    return (
      <p className="text-muted-foreground">
        Nothing to check out. <Link to="/" search={{}} className="underline underline-offset-4">Back to the shop</Link>
      </p>
    )
  }

  const shipping = shippingFor(cart.itemsTotal)
  const fields = [
    { name: 'customerName', label: 'Full name', autoComplete: 'name' },
    { name: 'street', label: 'Street and number', autoComplete: 'street-address' },
    { name: 'city', label: 'City', autoComplete: 'address-level2' },
    { name: 'zip', label: 'Postcode', autoComplete: 'postal-code' },
  ] as const

  return (
    <div className="space-y-8">
      <h1 className="text-3xl font-bold">Checkout</h1>
      <form
        onSubmit={form.handleSubmit((values) => place.mutate(values))}
        noValidate
        className="grid gap-8 lg:grid-cols-[1fr_22rem] lg:items-start"
      >
        <Card>
          <CardHeader>
            <CardTitle>Delivery address</CardTitle>
          </CardHeader>
          <CardContent>
            <FieldGroup>
              {fields.map((f) => (
                <Field key={f.name} data-invalid={!!form.formState.errors[f.name]}>
                  <FieldLabel htmlFor={f.name}>{f.label}</FieldLabel>
                  <Input id={f.name} autoComplete={f.autoComplete} {...form.register(f.name)} />
                  <FieldError errors={[form.formState.errors[f.name]]} />
                </Field>
              ))}
            </FieldGroup>
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Your order</CardTitle>
          </CardHeader>
          <CardContent className="space-y-5">
            <ul className="space-y-2 text-sm">
              {cart.lines.map((l) => (
                <li key={l.productId} className="flex justify-between gap-4">
                  <span>
                    {l.quantity} × {l.name}
                  </span>
                  <span className="tabular">{formatMoney(l.price * l.quantity)}</span>
                </li>
              ))}
            </ul>
            <OrderSummary itemsTotal={cart.itemsTotal} shipping={shipping} total={cart.itemsTotal + shipping} />
            {place.isError && (
              <Alert variant="destructive">
                <AlertDescription>{place.error.message}</AlertDescription>
              </Alert>
            )}
            <Button type="submit" size="lg" className="w-full" disabled={place.isPending || place.isSuccess}>
              {(place.isPending || place.isSuccess) && <Spinner />}
              Place order and pay
            </Button>
            <p className="text-xs text-muted-foreground">You’ll pay on the payment provider’s page and come straight back.</p>
          </CardContent>
        </Card>
      </form>
    </div>
  )
}
