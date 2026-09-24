import { useQueryClient } from '@tanstack/react-query'
import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from 'react'
import type { LoginResponse, User } from '../../shared/types/api'
import { getCurrentUser } from './api'
import { AuthContext, type AuthContextValue } from './auth-context'
import { clearSession, readSession, writeSession } from './auth-storage'
import { ApiError } from '../../shared/api/client'
import { ErrorState } from '../../shared/components/Feedback'
import { Button } from '../../shared/components/Button'

export function AuthProvider({ children }: { children: ReactNode }) {
  const queryClient = useQueryClient()
  const [initialSession] = useState(readSession)
  const [session, setSession] = useState<LoginResponse | null>(initialSession)
  const [isBootstrapping, setIsBootstrapping] = useState(Boolean(initialSession))
  const [bootstrapFailed, setBootstrapFailed] = useState(false)
  const [attempt, setAttempt] = useState(0)
  const bootstrapRequest = useRef<AbortController | null>(null)

  const signOut = useCallback(() => {
    bootstrapRequest.current?.abort()
    setBootstrapFailed(false)
    setIsBootstrapping(false)
    clearSession()
    setSession(null)
    queryClient.clear()
  }, [queryClient])

  const signIn = useCallback((nextSession: LoginResponse) => {
    bootstrapRequest.current?.abort()
    setBootstrapFailed(false)
    setIsBootstrapping(false)
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
    bootstrapRequest.current = controller
    getCurrentUser(controller.signal)
      .then((user) => { if (!controller.signal.aborted) updateUser(user) })
      .catch((error: unknown) => {
        if (controller.signal.aborted) return
        if (error instanceof ApiError && (error.status === 401 || error.status === 404)) signOut()
        else setBootstrapFailed(true)
      })
      .finally(() => {
        if (!controller.signal.aborted) setIsBootstrapping(false)
      })

    return () => controller.abort()
  }, [attempt, initialSession, signOut, updateUser])

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
    window.addEventListener('rivulus:unauthorized', signOut)
    return () => window.removeEventListener('rivulus:unauthorized', signOut)
  }, [signOut])

  const value = useMemo<AuthContextValue>(() => ({
    user: session?.user ?? null,
    isAuthenticated: Boolean(session),
    isBootstrapping,
    signIn,
    updateUser,
    signOut,
  }), [isBootstrapping, session, signIn, signOut, updateUser])

  return <AuthContext.Provider value={value}>{bootstrapFailed
    ? <main className="splash-screen"><ErrorState message="Não foi possível verificar sua sessão. Ela foi preservada; tente novamente quando a API estiver disponível." onRetry={() => {
      setBootstrapFailed(false)
      setIsBootstrapping(true)
      setAttempt(current => current + 1)
    }} /><Button variant="ghost" onClick={signOut}>Voltar ao login</Button></main>
    : children}</AuthContext.Provider>
}
