import type { QueryClient } from '@tanstack/react-query'

// Orval keys queries by URL (`/orders`, `/orders/{id}`, …). Invalidating by
// prefix refreshes a list together with every detail under it.
export function invalidate(queryClient: QueryClient, ...prefixes: string[]) {
  return queryClient.invalidateQueries({
    predicate: (q) => prefixes.some((p) => String(q.queryKey[0]).startsWith(p)),
  })
}
