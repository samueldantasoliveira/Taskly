import { createContext, useContext } from 'react'
import type { LoginResponse, User } from '../../shared/types/api'

export interface AuthContextValue {
  user: User | null
  isAuthenticated: boolean
  isBootstrapping: boolean
  signIn: (session: LoginResponse) => void
  updateUser: (user: User) => void
  signOut: () => void
}

export const AuthContext = createContext<AuthContextValue | null>(null)

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used inside AuthProvider')
  return context
}
