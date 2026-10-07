import { zodResolver } from '@hookform/resolvers/zod'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { createFileRoute, Link, useNavigate } from '@tanstack/react-router'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { toast } from 'sonner'
import { z } from 'zod'
import { getCouponsCodeCheck, postOrders } from '@/api/generated/endpoints'
import type { CouponCheck } from '@/api/generated/model'
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
  street: z.string().trim().regex(/\p{L}/u, 'Enter a street name.').regex(/[0-9]/, 'Enter a house number.'),
  city: z.string().trim().min(1, 'Enter a city.'),
  zip: z.string().trim().regex(/^(?:[0-9]{5}|[0-9]{3} [0-9]{2})$/, 'Enter a five-digit postcode.'),
})
type Address = z.infer<typeof schema>
type Placement = Address & { couponCode: string | null }

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
  const [code, setCode] = useState('')
  const [coupon, setCoupon] = useState<CouponCheck | null>(null)
  const check = useMutation({ mutationFn: (value: string) => getCouponsCodeCheck(value.trim()), onSuccess: setCoupon })

  const place = useMutation({
    mutationFn: ({ couponCode, ...address }: Placement) =>
      postOrders({
        ...address,
        items: cart.lines.map((l) => ({ productId: l.productId, quantity: l.quantity })),
        couponCode,
      }),
    onSuccess: async (order, { couponCode: _, ...address }) => {
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
  // The same rule as Ordering: a percentage of the items, delivery stays as it is.
  const discount = coupon ? Math.round(cart.itemsTotal * Number(coupon.discountPercent)) / 100 : 0
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
        onSubmit={form.handleSubmit((values) => place.mutate({ ...values, couponCode: coupon?.code ?? null }))}
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
            <OrderSummary
              itemsTotal={cart.itemsTotal}
              discount={discount}
              couponCode={coupon?.code}
              shipping={shipping}
              total={cart.itemsTotal - discount + shipping}
            />
            <Field data-invalid={check.isError}>
              <FieldLabel htmlFor="couponCode">Coupon code</FieldLabel>
              <div className="flex gap-2">
                <Input
                  id="couponCode"
                  value={coupon?.code ?? code}
                  disabled={!!coupon}
                  onChange={(e) => {
                    setCode(e.target.value)
                    check.reset()
                  }}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') {
                      e.preventDefault()
                      if (code.trim()) check.mutate(code)
                    }
                  }}
                />
                {coupon ? (
                  <Button type="button" variant="outline" onClick={() => setCoupon(null)}>
                    Remove
                  </Button>
                ) : (
                  <Button type="button" variant="outline" disabled={!code.trim() || check.isPending} onClick={() => check.mutate(code)}>
                    Apply
                  </Button>
                )}
              </div>
              {check.isError && <FieldError>{check.error.message}</FieldError>}
            </Field>
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
