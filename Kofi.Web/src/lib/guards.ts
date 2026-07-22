import { redirect, type ParsedLocation } from '@tanstack/react-router'
import { session } from '@/lib/session'

// Signing in is required; the role is checked by the page itself, so a signed-in
// person with the wrong role sees "no access" instead of being signed out.
export function requireUser(location: ParsedLocation) {
  const user = session.user()
  if (!user) {
    throw redirect({ to: '/login', search: { redirect: location.href } })
  }
  return { user }
}
