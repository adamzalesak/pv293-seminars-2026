import { session } from '@/lib/session'

// Every failure the API answers with ends up as one message the UI can show.
export class ApiError extends Error {
  readonly status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

export type ErrorType<_Error> = ApiError
export type BodyType<Body> = Body

export async function apiFetch<T>(url: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers)
  const token = session.token()
  if (token) headers.set('Authorization', `Bearer ${token}`)

  const response = await fetch(`/api${url}`, { ...init, headers })

  if (response.status === 401 && token) {
    session.expire()
  }
  if (!response.ok) {
    throw new ApiError(response.status, await errorMessage(response))
  }
  if (response.status === 204) return undefined as T

  const text = await response.text()
  if (!text) return undefined as T
  return (response.headers.get('content-type')?.includes('json') ? JSON.parse(text) : text) as T
}

async function errorMessage(response: Response): Promise<string> {
  if (response.status === 401) return 'Sign in to continue.'
  if (response.status === 403) return 'Your account does not have access to this.'
  if (response.status === 404) return 'Not found.'

  const text = await response.text()
  try {
    const body = JSON.parse(text)
    if (typeof body === 'string') return body
    return body.error ?? body.detail ?? body.title ?? fallback(response)
  } catch {
    return text.replace(/^Error: /, '') || fallback(response)
  }
}

const fallback = (response: Response) => `The shop answered ${response.status}.`
