import { zodResolver } from '@hookform/resolvers/zod'
import { createFileRoute, Link } from '@tanstack/react-router'
import { ArrowLeft, Mail, PackageCheck, Pencil, Plus, Trash2, XCircle } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import {
  getGetOrdersIdQueryOptions,
  useDeleteOrdersIdItemsItemId,
  useGetOrdersId,
  useGetProducts,
  usePostOrdersIdCancel,
  usePostOrdersIdItems,
  usePostOrdersIdResendConfirmation,
  usePostOrdersIdShip,
  usePutOrdersIdAddress,
} from '@/api/generated/endpoints'
import type { OrderDetail } from '@/api/generated/model'
import { OrderStatus } from '@/components/order-status'
import { OrderSummary } from '@/components/order-summary'
import { PageTitle } from '@/components/page-title'
import { ErrorState } from '@/components/states'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from '@/components/ui/alert-dialog'
import { Button } from '@/components/ui/button'
import { Card, CardAction, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog'
import { Field, FieldError, FieldGroup, FieldLabel } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { formatDateTime, formatMoney } from '@/lib/format'
import { useOnDone } from '@/lib/use-on-done'

export const Route = createFileRoute('/admin/orders/$orderId')({
  loader: ({ context, params }) => context.queryClient.prefetchQuery(getGetOrdersIdQueryOptions(params.orderId)),
  component: AdminOrder,
})

function AdminOrder() {
  const { orderId } = Route.useParams()
  const order = useGetOrdersId(orderId)
  const onDone = useOnDone()
  const ship = usePostOrdersIdShip({ mutation: onDone('Marked as shipped', '/orders') })
  const cancel = usePostOrdersIdCancel({ mutation: onDone('Order cancelled', '/orders', '/products') })
  const resend = usePostOrdersIdResendConfirmation({ mutation: onDone('Confirmation sent again') })
  const removeItem = useDeleteOrdersIdItemsItemId({ mutation: onDone('Item removed', '/orders', '/products') })

  if (order.isPending) return <Skeleton className="h-96 rounded-xl" />
  if (order.isError) return <ErrorState error={order.error} onRetry={() => order.refetch()} />
  const o = order.data
  // The shop refuses changes once a payment has started; the detail does not
  // say whether one has, so editing is offered while the order is new.
  const editable = o.status === 'New'

  return (
    <>
      <Button variant="ghost" size="sm" asChild className="mb-4 -ml-2">
        <Link to="/admin/orders">
          <ArrowLeft />
          Orders
        </Link>
      </Button>
      <PageTitle
        title={
          <span className="flex flex-wrap items-center gap-3">
            <span className="tabular">Order #{o.id!.slice(0, 8)}</span>
            <OrderStatus status={o.status} />
          </span>
        }
        description={`Placed ${formatDateTime(o.createdAt!)} by ${o.customerName} (${o.customerEmail})`}
        actions={
          <>
            <Button variant="outline" disabled={resend.isPending} onClick={() => resend.mutate({ id: orderId })}>
              <Mail />
              Resend confirmation
            </Button>
            {o.status !== 'Shipped' && o.status !== 'Cancelled' && (
              <AlertDialog>
                <AlertDialogTrigger asChild>
                  <Button variant="outline">
                    <XCircle />
                    Cancel order
                  </Button>
                </AlertDialogTrigger>
                <AlertDialogContent>
                  <AlertDialogHeader>
                    <AlertDialogTitle>Cancel this order?</AlertDialogTitle>
                    <AlertDialogDescription>
                      The items go back into stock, a payment already made is refunded and the customer gets an e-mail. This can’t be undone.
                    </AlertDialogDescription>
                  </AlertDialogHeader>
                  <AlertDialogFooter>
                    <AlertDialogCancel>Keep order</AlertDialogCancel>
                    <AlertDialogAction variant="destructive" onClick={() => cancel.mutate({ id: orderId })}>
                      Cancel order
                    </AlertDialogAction>
                  </AlertDialogFooter>
                </AlertDialogContent>
              </AlertDialog>
            )}
            {o.status === 'Paid' && (
              <Button disabled={ship.isPending} onClick={() => ship.mutate({ id: orderId })}>
                <PackageCheck />
                Mark as shipped
              </Button>
            )}
          </>
        }
      />

      <div className="grid gap-6 lg:grid-cols-[1fr_20rem] lg:items-start">
        <Card>
          <CardHeader>
            <CardTitle>Items</CardTitle>
            {editable && (
              <CardAction>
                <AddItemDialog orderId={orderId} />
              </CardAction>
            )}
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Product</TableHead>
                  <TableHead className="text-right">Qty</TableHead>
                  <TableHead className="text-right">Unit price</TableHead>
                  <TableHead className="text-right">Line</TableHead>
                  {editable && <TableHead className="w-10" />}
                </TableRow>
              </TableHeader>
              <TableBody>
                {o.items?.map((i) => (
                  <TableRow key={i.id}>
                    <TableCell>{i.productName}</TableCell>
                    <TableCell className="text-right tabular">{i.quantity}</TableCell>
                    <TableCell className="text-right tabular">{formatMoney(i.unitPrice ?? 0)}</TableCell>
                    <TableCell className="text-right tabular">{formatMoney(Number(i.unitPrice) * Number(i.quantity))}</TableCell>
                    {editable && (
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="icon-sm"
                          aria-label={`Remove ${i.productName}`}
                          disabled={removeItem.isPending}
                          onClick={() => removeItem.mutate({ id: orderId, itemId: i.id! })}
                        >
                          <Trash2 />
                        </Button>
                      </TableCell>
                    )}
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            <OrderSummary
              className="mt-4 ml-auto max-w-64"
              itemsTotal={Number(o.itemsTotal)}
              shipping={Number(o.shipping)}
              total={Number(o.total)}
            />
          </CardContent>
        </Card>
        <Card>
          <CardHeader>
            <CardTitle>Delivery address</CardTitle>
            <CardAction>
              <AddressDialog order={o} />
            </CardAction>
          </CardHeader>
          <CardContent>
            <address className="text-sm not-italic">
              {o.customerName}
              <br />
              {o.street}
              <br />
              {o.zip} {o.city}
            </address>
          </CardContent>
        </Card>
      </div>
    </>
  )
}

const addressSchema = z.object({
  street: z.string().trim().min(1, 'Enter a street.'),
  city: z.string().trim().min(1, 'Enter a city.'),
  zip: z.string().trim().min(3, 'Enter a postcode.'),
})

function AddressDialog({ order }: { order: OrderDetail }) {
  const [open, setOpen] = useState(false)
  const onDone = useOnDone()
  const form = useForm({
    resolver: zodResolver(addressSchema),
    values: { street: order.street ?? '', city: order.city ?? '', zip: order.zip ?? '' },
  })
  const save = usePutOrdersIdAddress({ mutation: onDone('Address updated', '/orders') })

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="ghost" size="icon-sm" aria-label="Change address">
          <Pencil />
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Change delivery address</DialogTitle>
        </DialogHeader>
        <form
          noValidate
          onSubmit={form.handleSubmit((data) => save.mutate({ id: order.id!, data }, { onSuccess: () => setOpen(false) }))}
        >
          <FieldGroup>
            {(['street', 'city', 'zip'] as const).map((name) => (
              <Field key={name} data-invalid={!!form.formState.errors[name]}>
                <FieldLabel htmlFor={`address-${name}`}>{{ street: 'Street', city: 'City', zip: 'Postcode' }[name]}</FieldLabel>
                <Input id={`address-${name}`} {...form.register(name)} />
                <FieldError errors={[form.formState.errors[name]]} />
              </Field>
            ))}
          </FieldGroup>
          <DialogFooter className="mt-6">
            <Button type="submit" disabled={save.isPending}>
              Save address
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function AddItemDialog({ orderId }: { orderId: string }) {
  const [open, setOpen] = useState(false)
  const [productId, setProductId] = useState('')
  const [quantity, setQuantity] = useState(1)
  const onDone = useOnDone()
  const products = useGetProducts({ pageSize: 100 }, { query: { enabled: open } })
  const add = usePostOrdersIdItems({ mutation: onDone('Item added', '/orders', '/products') })

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger asChild>
        <Button variant="outline" size="sm">
          <Plus />
          Add item
        </Button>
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Add an item</DialogTitle>
        </DialogHeader>
        <form
          onSubmit={(e) => {
            e.preventDefault()
            add.mutate({ id: orderId, data: { productId, quantity } }, { onSuccess: () => setOpen(false) })
          }}
        >
          <FieldGroup>
            <Field>
              <FieldLabel>Product</FieldLabel>
              <Select value={productId} onValueChange={setProductId}>
                <SelectTrigger className="w-full" aria-label="Product">
                  <SelectValue placeholder="Choose a product" />
                </SelectTrigger>
                <SelectContent>
                  {products.data?.items?.map((p) => (
                    <SelectItem key={p.id} value={p.id!}>
                      {p.name} ({formatMoney(p.price ?? 0)})
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </Field>
            <Field>
              <FieldLabel htmlFor="add-qty">Quantity</FieldLabel>
              <Input id="add-qty" type="number" min={1} value={quantity} onChange={(e) => setQuantity(Number(e.target.value))} />
            </Field>
          </FieldGroup>
          <DialogFooter className="mt-6">
            <Button type="submit" disabled={!productId || quantity < 1 || add.isPending}>
              Add item
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
