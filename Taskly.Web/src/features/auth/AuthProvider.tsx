import { useQueryClient } from '@tanstack/react-query'
import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import type { LoginResponse, User } from '../../shared/types/api'
import { getCurrentUser } from './api'
import { AuthContext, type AuthContextValue } from './auth-context'
import { clearSession, readSession, writeSession } from './auth-storage'

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const [initialSession] = useState(readSession)
  const [session, setSession] = useState<LoginResponse | null>(initialSession)
  const [isBootstrapping, setIsBootstrapping] = useState(Boolean(initialSession))

  const signOut = useCallback(() => {
    clearSession()
    setSession(null)
    queryClient.clear()
  }, [queryClient])

  const signIn = useCallback((nextSession: LoginResponse) => {
    writeSession(nextSession)
    setSession(nextSession)
  }, [])

  const updateUser = useCallback((user: User) => {
    setSession((current) => {
      if (!current) return current
      const next = { ...current, user }
      writeSession(next)
      return next
    })
  }, [])

  useEffect(() => {
    if (!initialSession) return

    const controller = new AbortController()
    getCurrentUser(controller.signal)
      .then(updateUser)
      .catch(() => {
        if (!controller.signal.aborted) signOut()
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsBootstrapping(false)
      })

    return () => controller.abort()
  }, [initialSession, signOut, updateUser])

  useEffect(() => {
    if (!session) return
    const maximumTimeout = 2_147_483_647
    let timeout: number

    const scheduleExpiration = () => {
      const remaining = new Date(session.expiresAt).getTime() - Date.now()
      timeout = window.setTimeout(() => {
        if (remaining <= maximumTimeout) signOut()
        else scheduleExpiration()
      }, Math.max(0, Math.min(remaining, maximumTimeout)))
    }

    scheduleExpiration()
    return () => window.clearTimeout(timeout)
  }, [session, signOut])

  useEffect(() => {
    window.addEventListener('taskly:unauthorized', signOut)
    return () => window.removeEventListener('taskly:unauthorized', signOut)
  }, [signOut])

  const value = useMemo<AuthContextValue>(() => ({
    user: session?.user ?? null,
    isAuthenticated: Boolean(session),
    isBootstrapping,
    signIn,
    updateUser,
    signOut,
  }), [isBootstrapping, session, signIn, signOut, updateUser])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
