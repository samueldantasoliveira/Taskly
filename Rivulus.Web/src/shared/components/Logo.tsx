import { cn } from '../lib/cn'

interface LogoProps {
  compact?: boolean
  className?: string
}

export function Logo({ compact = false, className }: LogoProps) {
  return (
    <div className={cn('logo', className)} aria-label="Rivulus">
      <span className="logo__mark" aria-hidden="true">
        <img src="/brand/rivulus-mark.svg" alt="" width="38" height="38" />
      </span>
      {!compact && <span className="logo__wordmark">Rivulus</span>}
    </div>
  )
}
