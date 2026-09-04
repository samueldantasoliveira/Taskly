import { describe, expect, it, vi } from 'vitest'
import type { LoginResponse } from '../../shared/types/api'
import { clearSession, getAccessToken, readSession, writeSession } from './auth-storage'

const validSession: LoginResponse = {
  token: 'token-de-teste',
  expiresAt: '2099-01-01T00:00:00.000Z',
  user: { id: 'user-1', name: 'Samuel', email: 'samuel@taskly.dev' },
}

describe('auth storage', () => {
  it('persiste e recupera uma sessão válida', () => {
    writeSession(validSession)

    expect(readSession()).toEqual(validSession)
    expect(getAccessToken()).toBe('token-de-teste')
  })

  it('remove uma sessão expirada', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2030-01-01T00:00:00.000Z'))
    writeSession({ ...validSession, expiresAt: '2029-12-31T23:59:59.000Z' })

    expect(readSession()).toBeNull()
    expect(sessionStorage.getItem('taskly.session')).toBeNull()
    vi.useRealTimers()
  })

  it('limpa a sessão explicitamente', () => {
    writeSession(validSession)
    clearSession()

    expect(readSession()).toBeNull()
  })
})
