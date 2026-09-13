import { Moon, Sun } from 'lucide-react'
import { useSyncExternalStore } from 'react'
import { getTheme, setTheme, subscribeToTheme } from '../lib/theme'

export function ThemeToggle() {
  const theme = useSyncExternalStore(subscribeToTheme, getTheme)
  const label = theme === 'dark' ? 'Ativar modo claro' : 'Ativar modo noturno'
  return (
    <button
      type="button"
      className="theme-toggle"
      aria-label={label}
      title={label}
      onClick={() => setTheme(theme === 'dark' ? 'light' : 'dark')}
    >
      {theme === 'dark' ? <Sun size={18} aria-hidden="true" /> : <Moon size={18} aria-hidden="true" />}
      <span>{theme === 'dark' ? 'Modo claro' : 'Modo noturno'}</span>
    </button>
  )
}
