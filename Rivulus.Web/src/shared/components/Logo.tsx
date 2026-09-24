import { cn } from '../lib/cn'

interface LogoProps {
  compact?: boolean
  className?: string
}

export function Logo({ compact = false, className }: LogoProps) {
  return (
    <div className={cn('logo', className)} aria-label="Rivulus">
      <span className="logo__mark" aria-hidden="true">
        <svg viewBox="0 0 32 32" role="presentation">
          <path d="M9 26V7.5h8.2c4.2 0 6.8 2.2 6.8 5.7s-2.7 5.8-6.8 5.8H9" />
          <path d="m17 19 7 7" />
        </svg>
      </span>
      {!compact && <span className="logo__wordmark">Rivulus</span>}
    </div>
  )
}
