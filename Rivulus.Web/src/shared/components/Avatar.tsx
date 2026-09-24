import { cn } from '../lib/cn'

function initials(name: string) {
  return name
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('')
}

export function Avatar({ name, size = 'md' }: { name: string; size?: 'sm' | 'md' | 'lg' }) {
  return (
    <span className={cn('avatar', `avatar--${size}`)} aria-label={name}>
      {initials(name)}
    </span>
  )
}
