import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { createRouter, RouterProvider } from '@tanstack/react-router'
import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { toast } from 'sonner'
import { ApiError } from '@/api/fetcher'
import { CartProvider } from '@/lib/cart'
import { session } from '@/lib/session'
import '@/lib/theme'
import { routeTree } from './routeTree.gen'
import './index.css'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 15_000,
      // A 4xx will not get better by asking again.
      retry: (count, error) => !(error instanceof ApiError && error.status < 500) && count < 2,
    },
  },
})

const router = createRouter({
  routeTree,
  context: { queryClient },
  // Hovering a link runs its route's loader, which warms the query cache;
  // React Query, not the router, decides whether the data is still fresh.
  defaultPreload: 'intent',
  defaultPreloadStaleTime: 0,
  scrollRestoration: true,
})

declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router
  }
}

// Server data belongs to whoever is signed in: any change of user starts from an
// empty cache. A token the API rejected sends the person back to sign in.
let signedInAs = session.user()?.email
session.subscribe(() => {
  const email = session.user()?.email
  if (email === signedInAs) return
  signedInAs = email
  queryClient.clear()
  if (session.takeReason() === 'expired') {
    toast.info('Your session has ended. Sign in again to continue.')
    router.navigate({ to: '/login', search: { redirect: router.state.location.href } })
  }
})

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <CartProvider>
        <RouterProvider router={router} />
      </CartProvider>
    </QueryClientProvider>
  </StrictMode>,
)
