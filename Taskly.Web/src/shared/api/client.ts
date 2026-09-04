import { clearSession, getAccessToken } from '../../features/auth/auth-storage'

const apiUrl = (import.meta.env.VITE_API_URL ?? 'http://localhost:5219')
  .replace(/\/$/, '')

export class ApiError extends Error {
  readonly status: number
  readonly details?: unknown

  constructor(
    message: string,
    status: number,
    details?: unknown,
  ) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.details = details
  }
}

function extractMessage(body: unknown, fallback: string): string {
  if (typeof body === 'string' && body.trim()) return body
  if (!body || typeof body !== 'object') return fallback

  const value = body as Record<string, unknown>
  if (typeof value.message === 'string') return value.message
  if (typeof value.title === 'string') return value.title

  if (value.errors && typeof value.errors === 'object') {
    const messages = Object.values(value.errors)
      .flatMap((error) => Array.isArray(error) ? error : [error])
      .filter((error): error is string => typeof error === 'string')

    if (messages.length) return messages.join(' ')
  }

  return fallback
}

async function readBody(response: Response): Promise<unknown> {
  if (response.status === 204) return undefined

  const text = await response.text()
  if (!text) return undefined

  try {
    return JSON.parse(text) as unknown
  } catch {
    return text
  }
}

export async function apiRequest<T>(
  path: string,
  init: RequestInit = {},
): Promise<T> {
  const token = getAccessToken()
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')

  if (init.body && !(init.body instanceof FormData)) {
    headers.set('Content-Type', 'application/json')
  }

  if (token) headers.set('Authorization', `Bearer ${token}`)

  const response = await fetch(`${apiUrl}${path}`, { ...init, headers })
  const body = await readBody(response)

  if (!response.ok) {
    if (response.status === 401 && token) {
      clearSession()
      window.dispatchEvent(new Event('taskly:unauthorized'))
    }

    throw new ApiError(
      extractMessage(body, 'Não foi possível concluir a solicitação.'),
      response.status,
      body,
    )
  }

  return body as T
}

export function jsonBody(value: unknown) {
  return JSON.stringify(value)
}
