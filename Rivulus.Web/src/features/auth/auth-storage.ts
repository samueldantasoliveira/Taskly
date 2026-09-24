import type { LoginResponse } from '../../shared/types/api'

const storageKey = 'rivulus.session'
const legacyStorageKey = 'taskly.session'

export function readSession(): LoginResponse | null {
  const value = sessionStorage.getItem(storageKey) ?? sessionStorage.getItem(legacyStorageKey)
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

    sessionStorage.setItem(storageKey, value)
    sessionStorage.removeItem(legacyStorageKey)
    return session
  } catch {
    clearSession()
    return null
  }
}

export function writeSession(session: LoginResponse) {
  sessionStorage.setItem(storageKey, JSON.stringify(session))
  sessionStorage.removeItem(legacyStorageKey)
}

export function clearSession() {
  sessionStorage.removeItem(storageKey)
  sessionStorage.removeItem(legacyStorageKey)
}

export function getAccessToken() {
  return readSession()?.token ?? null
}
