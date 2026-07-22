import { useSyncExternalStore } from 'react'

export type Theme = 'light' | 'dark' | 'system'

// index.html applies the stored theme before the first paint; this keeps it in
// sync afterwards. A per-browser preference, so localStorage is the right place.
const KEY = 'kofi.theme'
const media = window.matchMedia('(prefers-color-scheme: dark)')
const listeners = new Set<() => void>()

function stored(): Theme {
  try {
    const t = localStorage.getItem(KEY)
    return t === 'light' || t === 'dark' ? t : 'system'
  } catch {
    return 'system'
  }
}

let theme = stored()

const resolved = () => (theme === 'system' ? (media.matches ? 'dark' : 'light') : theme)

function apply() {
  document.documentElement.classList.toggle('dark', resolved() === 'dark')
  listeners.forEach((l) => l())
}

media.addEventListener('change', apply)
apply()

export function setTheme(next: Theme) {
  theme = next
  try {
    if (next === 'system') localStorage.removeItem(KEY)
    else localStorage.setItem(KEY, next)
  } catch {
    // Not remembered; it still applies for this visit.
  }
  apply()
}

const subscribe = (l: () => void) => {
  listeners.add(l)
  return () => listeners.delete(l)
}

export const useTheme = () => useSyncExternalStore(subscribe, () => theme)
export const useResolvedTheme = () => useSyncExternalStore(subscribe, resolved)
