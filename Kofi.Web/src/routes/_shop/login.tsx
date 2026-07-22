import { zodResolver } from '@hookform/resolvers/zod'
import { createFileRoute, useNavigate, useRouter } from '@tanstack/react-router'
import { useForm } from 'react-hook-form'
import { z } from 'zod'
import { usePostAuthLogin } from '@/api/generated/endpoints'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Field, FieldError, FieldGroup, FieldLabel } from '@/components/ui/field'
import { Input } from '@/components/ui/input'
import { Spinner } from '@/components/ui/spinner'
import { session } from '@/lib/session'

export const Route = createFileRoute('/_shop/login')({
  validateSearch: z.object({ redirect: z.string().optional() }),
  component: Login,
})

const schema = z.object({
  email: z.email('Enter an e-mail address.'),
  password: z.string().min(1, 'Enter your password.'),
})

// No sign-up: the shop has two demo accounts (see README).
const demoAccounts = [
  { label: 'Customer', email: 'petra@example.com', password: 'Customer123!' },
  { label: 'Administrator', email: 'admin@kofi.test', password: 'Admin123!' },
]

function Login() {
  const { redirect } = Route.useSearch()
  const navigate = useNavigate()
  const router = useRouter()
  const form = useForm({ resolver: zodResolver(schema), defaultValues: { email: '', password: '' } })

  const login = usePostAuthLogin({
    mutation: {
      onSuccess: (response) => {
        session.signIn(response.accessToken!)
        if (redirect) router.history.push(redirect)
        else navigate({ to: session.user()?.role === 'Administrator' ? '/admin' : '/' })
      },
    },
  })

  return (
    <div className="mx-auto max-w-sm py-6">
      <Card>
        <CardHeader>
          <CardTitle className="text-2xl">Sign in</CardTitle>
          <CardDescription>To place orders and see the ones you’ve made.</CardDescription>
        </CardHeader>
        <CardContent>
          <form onSubmit={form.handleSubmit((values) => login.mutate({ data: values }))} noValidate>
            <FieldGroup>
              {login.isError && (
                <Alert variant="destructive">
                  <AlertDescription>
                    {login.error.status === 401 ? 'That e-mail and password don’t match.' : login.error.message}
                  </AlertDescription>
                </Alert>
              )}
              <Field data-invalid={!!form.formState.errors.email}>
                <FieldLabel htmlFor="email">E-mail</FieldLabel>
                <Input id="email" type="email" autoComplete="username" {...form.register('email')} />
                <FieldError errors={[form.formState.errors.email]} />
              </Field>
              <Field data-invalid={!!form.formState.errors.password}>
                <FieldLabel htmlFor="password">Password</FieldLabel>
                <Input id="password" type="password" autoComplete="current-password" {...form.register('password')} />
                <FieldError errors={[form.formState.errors.password]} />
              </Field>
              <Button type="submit" disabled={login.isPending}>
                {login.isPending && <Spinner />}
                Sign in
              </Button>
            </FieldGroup>
          </form>
        </CardContent>
      </Card>
      <div className="mt-6 space-y-2 text-sm">
        <p className="text-muted-foreground">Demo accounts</p>
        {demoAccounts.map((a) => (
          <button
            key={a.email}
            type="button"
            className="flex w-full items-center justify-between rounded-lg border px-3 py-2 text-left transition-colors hover:bg-muted"
            onClick={() => {
              form.setValue('email', a.email)
              form.setValue('password', a.password)
            }}
          >
            <span>{a.label}</span>
            <span className="text-muted-foreground">{a.email}</span>
          </button>
        ))}
      </div>
    </div>
  )
}
