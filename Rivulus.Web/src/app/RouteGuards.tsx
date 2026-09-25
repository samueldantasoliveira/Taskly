import { Navigate, Outlet, useLocation } from 'react-router'
import { useAuth } from '../features/auth/auth-context'
import { Logo } from '../shared/components/Logo'
import { PageLoader } from '../shared/components/Feedback'

export function ProtectedRoute() {
  const { isAuthenticated, isBootstrapping } = useAuth()
  const location = useLocation()

  if (isBootstrapping) {
    return (
      <main className="splash-screen">
        <Logo />
        <PageLoader label="Preparando seu espaço de trabalho..." />
      </main>
    )
  }

  if (!isAuthenticated) {
    const destination = `${location.pathname}${location.search}${location.hash}`
    return <Navigate to="/login" replace state={{ from: destination }} />
  }

  return <Outlet />
}

export function PublicOnlyRoute() {
  const { isAuthenticated, isBootstrapping } = useAuth()
  const location = useLocation()
  if (isBootstrapping) return <div className="splash-screen"><PageLoader /></div>
  const destination = (location.state as { from?: string } | null)?.from ?? '/teams'
  return isAuthenticated ? <Navigate to={destination} replace /> : <Outlet />
}
