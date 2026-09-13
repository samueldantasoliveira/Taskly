// Apply the saved preference before the app is painted, including on direct links.
(() => {
  let theme = 'dark'
  try {
    if (localStorage.getItem('taskly.theme') === 'light') theme = 'light'
  } catch {
    // The default remains usable when browser storage is unavailable.
  }
  document.documentElement.dataset.theme = theme
  document.querySelector('meta[name="theme-color"]')?.setAttribute(
    'content', theme === 'dark' ? '#101813' : '#f5f7f5',
  )
})()
