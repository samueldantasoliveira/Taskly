import type { ReactNode } from 'react'
import { AlertCircle, Inbox, LoaderCircle } from 'lucide-react'
import { Button } from './Button'

export function PageLoader({ label = 'Carregando...' }: { label?: string }) {
  return (
    <div className="page-feedback" role="status">
      <LoaderCircle className="spin" size={28} />
      <span>{label}</span>
    </div>
  )
}

interface EmptyStateProps {
  title: string
  description: string
  action?: ReactNode
}

export function EmptyState({ title, description, action }: EmptyStateProps) {
  return (
    <div className="empty-state">
      <span className="empty-state__icon"><Inbox size={25} /></span>
      <h3>{title}</h3>
      <p>{description}</p>
      {action}
    </div>
  )
}

export function ErrorState({
  message,
  onRetry,
}: {
  message?: string
  onRetry?: () => void
}) {
  return (
    <div className="empty-state empty-state--error" role="alert">
      <span className="empty-state__icon"><AlertCircle size={25} /></span>
      <h3>Algo não saiu como esperado</h3>
      <p>{message ?? 'Não foi possível carregar estas informações.'}</p>
      {onRetry && <Button variant="secondary" onClick={onRetry}>Tentar novamente</Button>}
    </div>
  )
}
