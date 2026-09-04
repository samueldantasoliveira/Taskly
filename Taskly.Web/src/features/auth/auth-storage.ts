import type { LoginResponse } from '../../shared/types/api'

const storageKey = 'taskly.session'

export function readSession(): LoginResponse | null {
  const value = sessionStorage.getItem(storageKey)
  if (!value) return null

  try {
    const session = JSON.parse(value) as LoginResponse
    if (!session.token || !session.user || !session.expiresAt) {
      clearSession()
      return null
    }

    if (new Date(session.expiresAt).getTime() <= Date.now()) {
      clearSession()
      return null
    }

    return session
  } catch {
    clearSession()
    return null
  }
}

export function writeSession(session: LoginResponse) {
  sessionStorage.setItem(storageKey, JSON.stringify(session))
}

export function clearSession() {
  sessionStorage.removeItem(storageKey)
}

export function getAccessToken() {
  return readSession()?.token ?? null
}
