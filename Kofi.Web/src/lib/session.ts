import { useSyncExternalStore } from 'react'

// The token lives in localStorage: fine for a local demo, not for production
// (there it belongs in an HttpOnly cookie behind a BFF). The role read from it
// only drives navigation; the API enforces who may do what.

export type Role = 'Customer' | 'Administrator'

export type User = {
  email: string
  role: Role
  expiresAt: number
}

const KEY = 'kofi.token'
const listeners = new Set<() => void>()
let reason: 'expired' | null = null

function read(): { token: string; user: User } | null {
  let token: string | null = null
  try {
    token = localStorage.getItem(KEY)
  } catch {
    return null
  }
  if (!token) return null
  const user = decode(token)
  if (!user || user.expiresAt <= Date.now()) return null
  return { token, user }
}

function decode(token: string): User | null {
  try {
    const payload = JSON.parse(atob(token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')))
    return { email: payload.email, role: payload.role, expiresAt: payload.exp * 1000 }
  } catch {
    return null
  }
}

let current = read()

function write(token: string | null) {
  try {
    if (token) localStorage.setItem(KEY, token)
    else localStorage.removeItem(KEY)
  } catch {
    // Private mode: the session simply lasts as long as the tab.
  }
  current = token ? { token, user: decode(token)! } : null
  listeners.forEach((l) => l())
}

export const session = {
  token: () => (current && current.user.expiresAt > Date.now() ? current.token : null),
  user: (): User | null => {
    if (current && current.user.expiresAt <= Date.now()) write(null)
    return current?.user ?? null
  },
  signIn: (token: string) => {
    reason = null
    write(token)
  },
  signOut: () => write(null),
  // The API said 401 to a token we sent: it is no longer good.
  expire: () => {
    reason = 'expired'
    write(null)
  },
  takeReason: () => {
    const r = reason
    reason = null
    return r
  },
  subscribe: (listener: () => void) => {
    listeners.add(listener)
    return () => listeners.delete(listener)
  },
}

export function useUser() {
  return useSyncExternalStore(session.subscribe, session.user)
}
