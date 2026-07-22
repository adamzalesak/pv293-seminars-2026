import { zodResolver } from '@hookform/resolvers/zod'
import { createFileRoute, Link } from '@tanstack/react-router'
import { Plus } from 'lucide-react'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { getGetCustomersQueryOptions, useGetCustomers, usePostCustomers } from '@/api/generated/endpoints'
import { PageTitle } from '@/components/page-title'
import { ErrorState } from '@/components/states'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Dialog, DialogContent, DialogFooter, DialogHeader, DialogTitle } from '@/components/ui/dialog'
import { Field, FieldError, FieldGroup, FieldLabel } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table'
import { formatDate } from '@/lib/format'
import { useOnDone } from '@/lib/use-on-done'

export const Route = createFileRoute('/admin/customers/')({
  loader: ({ context }) => context.queryClient.prefetchQuery(getGetCustomersQueryOptions()),
  component: AdminCustomers,
})

function AdminCustomers() {
  const customers = useGetCustomers()
  const [creating, setCreating] = useState(false)

  return (
    <>
      <PageTitle
        title="Customers"
        actions={
          <Button onClick={() => setCreating(true)}>
            <Plus />
            New customer
          </Button>
        }
      />
      {customers.isError ? (
        <ErrorState error={customers.error} onRetry={() => customers.refetch()} />
      ) : customers.isPending ? (
        <Skeleton className="h-72 rounded-xl" />
      ) : (
        <div className="rounded-xl border bg-card">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>E-mail</TableHead>
                <TableHead>City</TableHead>
                <TableHead>Customer since</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {customers.data.map((c) => (
                <TableRow key={c.id}>
                  <TableCell>
                    <Link to="/admin/customers/$customerId" params={{ customerId: c.id! }} className="font-medium hover:underline">
                      {c.name}
                    </Link>
                    {c.isVip && (
                      <Badge variant="secondary" className="ml-2">
                        VIP
                      </Badge>
                    )}
                  </TableCell>
                  <TableCell className="text-muted-foreground">{c.email}</TableCell>
                  <TableCell>{c.city}</TableCell>
                  <TableCell className="text-muted-foreground">{formatDate(c.registeredAt!)}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}
      <CreateCustomerDialog open={creating} onOpenChange={setCreating} />
    </>
  )
}

const customerSchema = z.object({
  name: z.string().trim().min(1, 'Enter a name.'),
  email: z.email('Enter an e-mail address.'),
  street: z.string().trim(),
  city: z.string().trim(),
  zip: z.string().trim(),
})

const fields = [
  { name: 'name', label: 'Name' },
  { name: 'email', label: 'E-mail' },
  { name: 'street', label: 'Street' },
  { name: 'city', label: 'City' },
  { name: 'zip', label: 'Postcode' },
] as const

function CreateCustomerDialog({ open, onOpenChange }: { open: boolean; onOpenChange: (open: boolean) => void }) {
  const onDone = useOnDone()
  const form = useForm({
    resolver: zodResolver(customerSchema),
    defaultValues: { name: '', email: '', street: '', city: '', zip: '' },
  })
  const create = usePostCustomers({ mutation: onDone('Customer added', '/customers') })

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>New customer</DialogTitle>
        </DialogHeader>
        <form
          noValidate
          onSubmit={form.handleSubmit((data) =>
            create.mutate(
              { data },
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
            {fields.map((f) => (
              <Field key={f.name} data-invalid={!!form.formState.errors[f.name]}>
                <FieldLabel htmlFor={`c-${f.name}`}>{f.label}</FieldLabel>
                <Input id={`c-${f.name}`} {...form.register(f.name)} />
                <FieldError errors={[form.formState.errors[f.name]]} />
              </Field>
            ))}
          </FieldGroup>
          <DialogFooter className="mt-6">
            <Button type="submit" disabled={create.isPending}>
              Add customer
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
