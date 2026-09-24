// Apply the saved preference before the app is painted, including on direct links.
(() => {
  let theme = 'dark'
  try {
    const savedTheme = localStorage.getItem('rivulus.theme') ?? localStorage.getItem('taskly.theme')
    if (savedTheme === 'light') theme = 'light'
    if (savedTheme) {
      localStorage.setItem('rivulus.theme', savedTheme)
      localStorage.removeItem('taskly.theme')
    }
  } catch {
    // The default remains usable when browser storage is unavailable.
  }
  document.documentElement.dataset.theme = theme
  document.querySelector('meta[name="theme-color"]')?.setAttribute(
    'content', theme === 'dark' ? '#071b24' : '#f3f8f8',
  )
})()
