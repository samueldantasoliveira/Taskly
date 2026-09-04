import { CheckCheck } from 'lucide-react'
import { cn } from '../lib/cn'

interface LogoProps {
  compact?: boolean
  className?: string
}

export function Logo({ compact = false, className }: LogoProps) {
  return (
    <div className={cn('logo', className)} aria-label="Taskly">
      <span className="logo__mark"><CheckCheck size={22} /></span>
      {!compact && <span>taskly</span>}
    </div>
  )
}
