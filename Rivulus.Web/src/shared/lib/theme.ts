export type Theme = 'dark' | 'light'

const storageKey = 'rivulus.theme'
const changeEvent = 'rivulus:theme-change'

export function getTheme(): Theme {
  return document.documentElement.dataset.theme === 'light' ? 'light' : 'dark'
}

function applyTheme(theme: Theme) {
  document.documentElement.dataset.theme = theme
  document.querySelector('meta[name="theme-color"]')?.setAttribute(
    'content', theme === 'dark' ? '#071b24' : '#f3f8f8',
  )
}

export function setTheme(theme: Theme) {
  applyTheme(theme)
  try {
    localStorage.setItem(storageKey, theme)
  } catch {
    // Changing the current theme must still work without persistent storage.
  }
  window.dispatchEvent(new Event(changeEvent))
}

export function subscribeToTheme(onChange: () => void) {
  const onStorage = (event: StorageEvent) => {
    if (event.key !== storageKey && event.key !== null) return
    applyTheme(event.newValue === 'light' ? 'light' : 'dark')
    onChange()
  }
  window.addEventListener(changeEvent, onChange)
  window.addEventListener('storage', onStorage)
  return () => {
    window.removeEventListener(changeEvent, onChange)
    window.removeEventListener('storage', onStorage)
  }
}
