import { useQueryClient } from '@tanstack/react-query'
import { toast } from 'sonner'
import { invalidate } from '@/lib/query'

// Mutation callbacks for admin actions: confirm, refresh what changed, or say
// what went wrong in the shop's own words.
export function useOnDone() {
  const queryClient = useQueryClient()
  return (message: string, ...prefixes: string[]) => ({
    onSuccess: () => {
      toast.success(message)
      return invalidate(queryClient, ...prefixes)
    },
    onError: (error: Error) => {
      toast.error(error.message)
    },
  })
}
