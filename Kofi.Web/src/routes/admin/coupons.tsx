import { zodResolver } from '@hookform/resolvers/zod'
import { createFileRoute } from '@tanstack/react-router'
import { MoreHorizontal, Plus } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import {
  getGetCouponsQueryOptions,
  useGetCoupons,
  usePostCoupons,
  usePostCouponsIdInvalidate,
  usePutCouponsId,
} from '@/api/generated/endpoints'
import type { CouponSummary } from '@/api/generated/model'
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
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from '@/components/ui/dropdown-menu'
import { Field, FieldError, FieldGroup, FieldLabel } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { formatDate, isoDay } from '@/lib/format'
import { useOnDone } from '@/lib/use-on-done'

export const Route = createFileRoute('/admin/coupons')({
  loader: ({ context }) => context.queryClient.prefetchQuery(getGetCouponsQueryOptions()),
  component: AdminCoupons,
})

type Action = { kind: 'edit' | 'invalidate'; coupon: CouponSummary } | null

function AdminCoupons() {
  const coupons = useGetCoupons()
  const [creating, setCreating] = useState(false)
  const [action, setAction] = useState<Action>(null)

  return (
    <>
      <PageTitle
        title="Coupons"
        description="Coupons issued by hand and referral codes from new subscriptions."
        actions={
          <Button onClick={() => setCreating(true)}>
            <Plus />
            New coupon
          </Button>
        }
      />
      {coupons.isError ? (
        <ErrorState error={coupons.error} onRetry={() => coupons.refetch()} />
      ) : coupons.isPending ? (
        <Skeleton className="h-96 rounded-xl" />
      ) : (
        <div className="rounded-xl border bg-card">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Code</TableHead>
                <TableHead>Kind</TableHead>
                <TableHead className="text-right">Discount</TableHead>
                <TableHead>Valid until</TableHead>
                <TableHead className="text-right">Used</TableHead>
                <TableHead className="w-10" />
              </TableRow>
            </TableHeader>
            <TableBody>
              {coupons.data.map((c) => (
                <TableRow key={c.id} className={c.isInvalidated ? 'text-muted-foreground' : undefined}>
                  <TableCell className="font-mono font-medium">
                    {c.code}
                    {c.isInvalidated && <span className="ml-2 font-sans text-xs font-normal">Invalidated</span>}
                  </TableCell>
                  <TableCell className="text-muted-foreground">{c.reason === 'Referral' ? 'Referral' : 'Manual'}</TableCell>
                  <TableCell className="text-right tabular">{c.discountPercent} %</TableCell>
                  <TableCell className="text-muted-foreground">{formatDate(c.expiresAt)}</TableCell>
                  <TableCell className="text-right tabular">{c.useCount}</TableCell>
                  <TableCell>
                    {!c.isInvalidated && (
                      <DropdownMenu>
                        <DropdownMenuTrigger asChild>
                          <Button variant="ghost" size="icon-sm" aria-label={`Actions for ${c.code}`}>
                            <MoreHorizontal />
                          </Button>
                        </DropdownMenuTrigger>
                        <DropdownMenuContent align="end">
                          <DropdownMenuItem onSelect={() => setAction({ kind: 'edit', coupon: c })}>Edit</DropdownMenuItem>
                          <DropdownMenuItem variant="destructive" onSelect={() => setAction({ kind: 'invalidate', coupon: c })}>
                            Invalidate
                          </DropdownMenuItem>
                        </DropdownMenuContent>
                      </DropdownMenu>
                    )}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
      <CouponDialog open={creating} onClose={() => setCreating(false)} />
      {action?.kind === 'edit' && <CouponDialog open coupon={action.coupon} onClose={() => setAction(null)} />}
      {action?.kind === 'invalidate' && <InvalidateDialog coupon={action.coupon} onClose={() => setAction(null)} />}
    </>
  )
}

const couponSchema = z.object({
  code: z.string().trim().regex(/^[A-Za-z0-9-]{4,20}$/, 'Use 4 to 20 letters, digits or dashes.'),
  discountPercent: z.coerce
    .number<string>()
    .int('Enter a whole number.')
    .min(1, 'At least 1 %.')
    .max(50, 'At most 50 %.'),
  expiresAt: z.string().min(1, 'Choose a day.'),
})

// The code is set once; editing changes only the discount and the validity.
function CouponDialog({ open, coupon, onClose }: { open: boolean; coupon?: CouponSummary; onClose: () => void }) {
  const onDone = useOnDone()
  const form = useForm({
    resolver: zodResolver(couponSchema),
    values: {
      code: coupon?.code ?? '',
      discountPercent: String(coupon?.discountPercent ?? '10'),
      expiresAt: coupon ? coupon.expiresAt.slice(0, 10) : isoDay(30),
    },
  })
  const issue = usePostCoupons({ mutation: onDone('Coupon issued', '/coupons') })
  const update = usePutCouponsId({ mutation: onDone('Coupon updated', '/coupons') })
  const err = form.formState.errors
  const close = () => {
    form.reset()
    onClose()
  }

  return (
    <Dialog open={open} onOpenChange={(o) => !o && close()}>
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>{coupon ? `Edit ${coupon.code}` : 'New coupon'}</DialogTitle>
          <DialogDescription>A customer uses a coupon once; the discount applies to the items, not delivery.</DialogDescription>
        </DialogHeader>
        <form
          noValidate
          onSubmit={form.handleSubmit((v) => {
            const terms = { discountPercent: v.discountPercent, expiresAt: `${v.expiresAt}T23:59:59` }
            if (coupon) update.mutate({ id: coupon.id, data: terms }, { onSuccess: close })
            else issue.mutate({ data: { code: v.code, ...terms } }, { onSuccess: close })
          })}
        >
          <FieldGroup>
            <Field data-invalid={!!err.code}>
              <FieldLabel htmlFor="c-code">Code</FieldLabel>
              <Input id="c-code" disabled={!!coupon} {...form.register('code')} />
              <FieldError errors={[err.code]} />
            </Field>
            <div className="grid grid-cols-2 gap-4">
              <Field data-invalid={!!err.discountPercent}>
                <FieldLabel htmlFor="c-discount">Discount (%)</FieldLabel>
                <Input id="c-discount" inputMode="numeric" {...form.register('discountPercent')} />
                <FieldError errors={[err.discountPercent]} />
              </Field>
              <Field data-invalid={!!err.expiresAt}>
                <FieldLabel htmlFor="c-expires">Valid until</FieldLabel>
                <Input id="c-expires" type="date" {...form.register('expiresAt')} />
                <FieldError errors={[err.expiresAt]} />
              </Field>
            </div>
          </FieldGroup>
          <DialogFooter className="mt-6">
            <Button type="submit" disabled={issue.isPending || update.isPending}>
              {coupon ? 'Save changes' : 'Issue coupon'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function InvalidateDialog({ coupon, onClose }: { coupon: CouponSummary; onClose: () => void }) {
  const onDone = useOnDone()
  const invalidate = usePostCouponsIdInvalidate({ mutation: onDone('Coupon invalidated', '/coupons') })
  return (
    <AlertDialog open onOpenChange={(o) => !o && onClose()}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Invalidate {coupon.code}?</AlertDialogTitle>
          <AlertDialogDescription>Nobody can use it from now on. Orders that already used it keep their discount.</AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Keep it</AlertDialogCancel>
          <AlertDialogAction variant="destructive" onClick={() => invalidate.mutate({ id: coupon.id }, { onSuccess: onClose })}>
            Invalidate
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
