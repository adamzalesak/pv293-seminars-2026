import { useMutation } from '@tanstack/react-query'
import { createFileRoute, Link, useNavigate } from '@tanstack/react-router'
import { Download } from 'lucide-react'
import { toast } from 'sonner'
import { z } from 'zod'
import { getGetOrdersQueryOptions, getOrdersExport, useGetOrders } from '@/api/generated/endpoints'
import { OrderStatus } from '@/components/order-status'
import { PageTitle } from '@/components/page-title'
import { ErrorState } from '@/components/states'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { Skeleton } from '@/components/ui/skeleton'
import { Spinner } from '@/components/ui/spinner'
import { Switch } from '@/components/ui/switch'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { formatDateTime, formatMoney } from '@/lib/format'

export const Route = createFileRoute('/admin/orders/')({
  validateSearch: z.object({
    onlyPaid: z.boolean().optional().catch(undefined),
    includeItems: z.boolean().optional().catch(undefined),
    from: z.string().optional().catch(undefined),
    sortBy: z.enum(['date', 'total', 'customer']).optional().catch(undefined),
  }),
  loaderDeps: ({ search }) => search,
  loader: ({ context, deps }) => context.queryClient.prefetchQuery(getGetOrdersQueryOptions(deps)),
  component: AdminOrders,
})

function AdminOrders() {
  const search = Route.useSearch()
  const navigate = useNavigate({ from: Route.fullPath })
  const set = (patch: Partial<typeof search>) => navigate({ search: (s) => ({ ...s, ...patch }) })
  const orders = useGetOrders(search)

  const exportCsv = useMutation({
    // The export is plain text, which the API does not describe; the fetcher hands it back as a string.
    mutationFn: () => getOrdersExport({ from: search.from }) as unknown as Promise<string>,
    onSuccess: (csv) => {
      const url = URL.createObjectURL(new Blob([csv], { type: 'text/csv' }))
      const a = Object.assign(document.createElement('a'), { href: url, download: 'kofi-orders.csv' })
      a.click()
      URL.revokeObjectURL(url)
    },
    onError: (e) => toast.error(e.message),
  })

  return (
    <>
      <PageTitle
        title="Orders"
        actions={
          <Button variant="outline" disabled={exportCsv.isPending} onClick={() => exportCsv.mutate()}>
            {exportCsv.isPending ? <Spinner /> : <Download />}
            Export CSV
          </Button>
        }
      />
      <div className="mb-5 flex flex-wrap items-end gap-x-6 gap-y-4">
        <div className="grid gap-1.5">
          <Label htmlFor="from">Placed since</Label>
          <Input
            id="from"
            type="date"
            className="w-44"
            value={search.from ?? ''}
            onChange={(e) => set({ from: e.target.value || undefined })}
          />
        </div>
        <div className="grid gap-1.5">
          <Label>Sort by</Label>
          <Select value={search.sortBy ?? 'date'} onValueChange={(v) => set({ sortBy: v === 'date' ? undefined : (v as 'total') })}>
            <SelectTrigger className="w-44" aria-label="Sort by">
              <SelectValue />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="date">Newest first</SelectItem>
              <SelectItem value="total">Highest total</SelectItem>
              <SelectItem value="customer">Customer name</SelectItem>
            </SelectContent>
          </Select>
        </div>
        <div className="flex h-9 items-center gap-2">
          <Switch id="only-paid" checked={!!search.onlyPaid} onCheckedChange={(v) => set({ onlyPaid: v || undefined })} />
          <Label htmlFor="only-paid">Paid and shipped only</Label>
        </div>
        <div className="flex h-9 items-center gap-2">
          <Switch id="items" checked={!!search.includeItems} onCheckedChange={(v) => set({ includeItems: v || undefined })} />
          <Label htmlFor="items">Show items</Label>
        </div>
      </div>

      {orders.isError ? (
        <ErrorState error={orders.error} onRetry={() => orders.refetch()} />
      ) : orders.isPending ? (
        <Skeleton className="h-72 rounded-xl" />
      ) : (
        <div className="rounded-xl border bg-card">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Order</TableHead>
                <TableHead>Customer</TableHead>
                <TableHead>Placed</TableHead>
                {search.includeItems && <TableHead>Items</TableHead>}
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Total</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {orders.data.map((o) => (
                <TableRow key={o.id}>
                  <TableCell>
                    <Link to="/admin/orders/$orderId" params={{ orderId: o.id! }} className="font-medium tabular hover:underline">
                      #{o.id!.slice(0, 8)}
                    </Link>
                  </TableCell>
                  <TableCell>
                    <div>{o.customerName}</div>
                    <div className="text-xs text-muted-foreground">{o.customerEmail}</div>
                  </TableCell>
                  <TableCell className="text-muted-foreground">{formatDateTime(o.createdAt!)}</TableCell>
                  {search.includeItems && (
                    <TableCell className="max-w-64 text-sm whitespace-normal">
                      {o.items?.map((i) => `${i.quantity}× ${i.productName}`).join(', ')}
                    </TableCell>
                  )}
                  <TableCell>
                    <OrderStatus status={o.status} />
                  </TableCell>
                  <TableCell className="text-right tabular">{formatMoney(o.total ?? 0)}</TableCell>
                </TableRow>
              ))}
              {orders.data.length === 0 && (
                <TableRow>
                  <TableCell colSpan={6} className="py-10 text-center text-muted-foreground">
                    No orders match these filters.
                  </TableCell>
                </TableRow>
              )}
            </TableBody>
          </Table>
        </div>
      )}
    </>
  )
}
