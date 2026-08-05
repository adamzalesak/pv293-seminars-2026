import { zodResolver } from '@hookform/resolvers/zod'
import { createFileRoute } from '@tanstack/react-router'
import { MoreHorizontal, Plus } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import {
  getGetProductsQueryOptions,
  useGetProducts,
  useGetProductsId,
  usePostProducts,
  usePostProductsIdActivate,
  usePostProductsIdDeactivate,
  usePostProductsIdRestock,
  usePutProductsId,
  usePutProductsIdPrice,
} from '@/api/generated/endpoints'
import type { ProductDetail, ProductSummary } from '@/api/generated/model'
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
} from '@/components/ui/alert-dialog'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu'
import { Field, FieldDescription, FieldError, FieldGroup, FieldLabel } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { Textarea } from '@/components/ui/textarea'
import { formatDate, formatMoney } from '@/lib/format'
import { useOnDone } from '@/lib/use-on-done'

const allProducts = { pageSize: 100, includeInactive: true }

export const Route = createFileRoute('/admin/products')({
  loader: ({ context }) => context.queryClient.prefetchQuery(getGetProductsQueryOptions(allProducts)),
  component: AdminProducts,
})

type Action = { kind: 'edit' | 'price' | 'restock' | 'deactivate'; product: ProductSummary } | null

function AdminProducts() {
  const products = useGetProducts(allProducts)
  const [creating, setCreating] = useState(false)
  const [action, setAction] = useState<Action>(null)
  const onDone = useOnDone()
  const activate = usePostProductsIdActivate({ mutation: onDone('Product back on sale', '/products') })

  return (
    <>
      <PageTitle
        title="Products"
        description="The whole catalogue, including products taken off sale."
        actions={
          <Button onClick={() => setCreating(true)}>
            <Plus />
            New product
          </Button>
        }
      />
      {products.isError ? (
        <ErrorState error={products.error} onRetry={() => products.refetch()} />
      ) : products.isPending ? (
        <Skeleton className="h-96 rounded-xl" />
      ) : (
        <div className="rounded-xl border bg-card">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Product</TableHead>
                <TableHead>Origin</TableHead>
                <TableHead>Roasted</TableHead>
                <TableHead className="text-right">Stock</TableHead>
                <TableHead className="text-right">Price</TableHead>
                <TableHead className="w-10" />
              </TableRow>
            </TableHeader>
            <TableBody>
              {products.data.items?.map((p) => (
                <TableRow key={p.id} className={p.isActive ? undefined : 'text-muted-foreground'}>
                  <TableCell className="font-medium">
                    {p.name}
                    {!p.isActive && <span className="ml-2 text-xs font-normal">Off sale</span>}
                  </TableCell>
                  <TableCell className="text-muted-foreground">{p.origin ?? '—'}</TableCell>
                  <TableCell className="text-muted-foreground">{p.roastedOn ? formatDate(p.roastedOn) : '—'}</TableCell>
                  <TableCell className="text-right tabular">{p.stockQuantity}</TableCell>
                  <TableCell className="text-right tabular">{formatMoney(p.price ?? 0)}</TableCell>
                  <TableCell>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon-sm" aria-label={`Actions for ${p.name}`}>
                          <MoreHorizontal />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem onSelect={() => setAction({ kind: 'edit', product: p })}>Edit details</DropdownMenuItem>
                        <DropdownMenuItem onSelect={() => setAction({ kind: 'price', product: p })}>Change price</DropdownMenuItem>
                        <DropdownMenuItem onSelect={() => setAction({ kind: 'restock', product: p })}>Restock</DropdownMenuItem>
                        <DropdownMenuSeparator />
                        {p.isActive ? (
                          <DropdownMenuItem variant="destructive" onSelect={() => setAction({ kind: 'deactivate', product: p })}>
                            Stop selling
                          </DropdownMenuItem>
                        ) : (
                          <DropdownMenuItem disabled={activate.isPending} onSelect={() => activate.mutate({ id: p.id! })}>
                            Resume selling
                          </DropdownMenuItem>
                        )}
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
      <CreateProductDialog open={creating} onOpenChange={setCreating} />
      {action?.kind === 'edit' && <EditProductDialog product={action.product} onClose={() => setAction(null)} />}
      {action?.kind === 'price' && <PriceDialog product={action.product} onClose={() => setAction(null)} />}
      {action?.kind === 'restock' && <RestockDialog product={action.product} onClose={() => setAction(null)} />}
      {action?.kind === 'deactivate' && <DeactivateDialog product={action.product} onClose={() => setAction(null)} />}
    </>
  )
}

const productSchema = z.object({
  name: z.string().trim().min(1, 'Enter a name.'),
  description: z.string().trim(),
  price: z.coerce.number<string>().positive('Enter a price above zero.'),
  stockQuantity: z.coerce.number<string>().int('Enter a whole number.').min(0, 'Stock can’t be negative.'),
  origin: z.string().trim(),
  roastedOn: z.string(),
})

function CreateProductDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (open: boolean) => void }) {
  const onDone = useOnDone()
  const form = useForm({
    resolver: zodResolver(productSchema),
    defaultValues: { name: '', description: '', price: '', stockQuantity: '0', origin: '', roastedOn: '' },
  })
  const create = usePostProducts({ mutation: onDone('Product added', '/products') })
  const err = form.formState.errors

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>New product</DialogTitle>
        </DialogHeader>
        <form
          noValidate
          onSubmit={form.handleSubmit((v) =>
            create.mutate(
              { data: { ...v, origin: v.origin || null, roastedOn: v.roastedOn || null } },
              {
                onSuccess: () => {
                  form.reset()
                  onOpenChange(false)
                },
              },
            ),
          )}
        >
          <FieldGroup>
            <Field data-invalid={!!err.name}>
              <FieldLabel htmlFor="p-name">Name</FieldLabel>
              <Input id="p-name" {...form.register('name')} />
              <FieldError errors={[err.name]} />
            </Field>
            <Field>
              <FieldLabel htmlFor="p-desc">Description</FieldLabel>
              <Textarea id="p-desc" rows={2} {...form.register('description')} />
            </Field>
            <div className="grid grid-cols-2 gap-4">
              <Field data-invalid={!!err.price}>
                <FieldLabel htmlFor="p-price">Price (EUR)</FieldLabel>
                <Input id="p-price" inputMode="decimal" {...form.register('price')} />
                <FieldError errors={[err.price]} />
              </Field>
              <Field data-invalid={!!err.stockQuantity}>
                <FieldLabel htmlFor="p-stock">Stock</FieldLabel>
                <Input id="p-stock" inputMode="numeric" {...form.register('stockQuantity')} />
                <FieldError errors={[err.stockQuantity]} />
              </Field>
              <Field>
                <FieldLabel htmlFor="p-origin">Origin</FieldLabel>
                <Input id="p-origin" {...form.register('origin')} />
              </Field>
              <Field>
                <FieldLabel htmlFor="p-roasted">Roasted on</FieldLabel>
                <Input id="p-roasted" type="date" {...form.register('roastedOn')} />
              </Field>
            </div>
            <FieldDescription>Leave origin and roast date empty for brewing kit.</FieldDescription>
          </FieldGroup>
          <DialogFooter className="mt-6">
            <Button type="submit" disabled={create.isPending}>
              Add product
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

const detailsSchema = productSchema.pick({ name: true, description: true, origin: true, roastedOn: true })

// The list leaves the description out, so the form starts from the product's detail.
function EditProductDialog({ product, onClose }: { product: ProductSummary; onClose: () => void }) {
  const detail = useGetProductsId(product.id)
  if (detail.data) return <EditProductForm product={detail.data} onClose={onClose} />
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Edit {product.name}</DialogTitle>
          <DialogDescription>Price and stock have their own actions.</DialogDescription>
        </DialogHeader>
        {detail.isError ? (
          <ErrorState error={detail.error} onRetry={() => detail.refetch()} />
        ) : (
          <Skeleton className="h-64 rounded-lg" />
        )}
      </DialogContent>
    </Dialog>
  )
}

function EditProductForm({ product, onClose }: { product: ProductDetail; onClose: () => void }) {
  const onDone = useOnDone()
  const form = useForm({
    resolver: zodResolver(detailsSchema),
    defaultValues: {
      name: product.name ?? '',
      description: product.description ?? '',
      origin: product.origin ?? '',
      roastedOn: product.roastedOn ?? '',
    },
  })
  const update = usePutProductsId({ mutation: onDone('Product updated', '/products') })
  const err = form.formState.errors

  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Edit {product.name}</DialogTitle>
          <DialogDescription>Price and stock have their own actions.</DialogDescription>
        </DialogHeader>
        <form
          noValidate
          onSubmit={form.handleSubmit((v) =>
            update.mutate(
              {
                id: product.id,
                data: { ...v, origin: v.origin || null, roastedOn: v.roastedOn || null, imageUrl: product.imageUrl },
              },
              { onSuccess: onClose },
            ),
          )}
        >
          <FieldGroup>
            <Field data-invalid={!!err.name}>
              <FieldLabel htmlFor="e-name">Name</FieldLabel>
              <Input id="e-name" {...form.register('name')} />
              <FieldError errors={[err.name]} />
            </Field>
            <Field>
              <FieldLabel htmlFor="e-desc">Description</FieldLabel>
              <Textarea id="e-desc" rows={2} {...form.register('description')} />
            </Field>
            <div className="grid grid-cols-2 gap-4">
              <Field>
                <FieldLabel htmlFor="e-origin">Origin</FieldLabel>
                <Input id="e-origin" {...form.register('origin')} />
              </Field>
              <Field>
                <FieldLabel htmlFor="e-roasted">Roasted on</FieldLabel>
                <Input id="e-roasted" type="date" {...form.register('roastedOn')} />
              </Field>
            </div>
          </FieldGroup>
          <DialogFooter className="mt-6">
            <Button type="submit" disabled={update.isPending}>
              Save changes
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function NumberDialog({
  title,
  description,
  label,
  initial,
  submit,
  pending,
  onSubmit,
  onClose,
}: {
  title: string
  description: string
  label: string
  initial: string
  submit: string
  pending: boolean
  onSubmit: (value: number) => void
  onClose: () => void
}) {
  const [value, setValue] = useState(initial)
  const n = Number(value)
  return (
    <Dialog open onOpenChange={(o) => !o && onClose()}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>
        <form
          onSubmit={(e) => {
            e.preventDefault()
            onSubmit(n)
          }}
        >
          <Field>
            <FieldLabel htmlFor="number-value">{label}</FieldLabel>
            <Input id="number-value" inputMode="decimal" autoFocus value={value} onChange={(e) => setValue(e.target.value)} />
          </Field>
          <DialogFooter className="mt-6">
            <Button type="submit" disabled={value === '' || Number.isNaN(n) || pending}>
              {submit}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function PriceDialog({ product, onClose }: { product: ProductSummary; onClose: () => void }) {
  const onDone = useOnDone()
  const change = usePutProductsIdPrice({ mutation: onDone('Price changed', '/products') })
  return (
    <NumberDialog
      title="Change price"
      description={product.name ?? ''}
      label="New price (EUR)"
      initial={String(product.price ?? '')}
      submit="Change price"
      pending={change.isPending}
      onSubmit={(price) => change.mutate({ id: product.id!, data: { price } }, { onSuccess: onClose })}
      onClose={onClose}
    />
  )
}

function RestockDialog({ product, onClose }: { product: ProductSummary; onClose: () => void }) {
  const onDone = useOnDone()
  const restock = usePostProductsIdRestock({ mutation: onDone('Stock added', '/products') })
  return (
    <NumberDialog
      title="Restock"
      description={`${product.name}: ${product.stockQuantity} in stock now.`}
      label="Units to add"
      initial="10"
      submit="Add stock"
      pending={restock.isPending}
      onSubmit={(quantity) => restock.mutate({ id: product.id!, data: { quantity } }, { onSuccess: onClose })}
      onClose={onClose}
    />
  )
}

function DeactivateDialog({ product, onClose }: { product: ProductSummary; onClose: () => void }) {
  const onDone = useOnDone()
  const deactivate = usePostProductsIdDeactivate({ mutation: onDone('Product taken off sale', '/products') })
  return (
    <AlertDialog open onOpenChange={(o) => !o && onClose()}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Stop selling {product.name}?</AlertDialogTitle>
          <AlertDialogDescription>
            It disappears from the shop; you can put it back on sale later. Orders already placed keep it.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Keep selling</AlertDialogCancel>
          <AlertDialogAction variant="destructive" onClick={() => deactivate.mutate({ id: product.id! }, { onSuccess: onClose })}>
            Stop selling
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
