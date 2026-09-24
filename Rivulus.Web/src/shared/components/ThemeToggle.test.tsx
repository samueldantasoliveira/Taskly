/// <reference types="node" />
import { runInNewContext } from 'node:vm'
import { act, render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { ThemeToggle } from './ThemeToggle'
import bootstrap from '../../../public/theme-init.js?raw'

function loadTheme() {
  runInNewContext(bootstrap, { document, localStorage })
}

beforeEach(() => {
  localStorage.clear()
  delete document.documentElement.dataset.theme
  const meta = document.createElement('meta')
  meta.name = 'theme-color'
  document.head.append(meta)
})

afterEach(() => {
  vi.restoreAllMocks()
  localStorage.clear()
  delete document.documentElement.dataset.theme
  document.querySelector('meta[name="theme-color"]')?.remove()
})

describe('ThemeToggle', () => {
  it('inicia no modo noturno quando não há preferência salva', () => {
    loadTheme()
    render(<ThemeToggle />)
    expect(document.documentElement).toHaveAttribute('data-theme', 'dark')
    expect(screen.getByRole('button', { name: 'Ativar modo claro' })).toBeInTheDocument()
    expect(document.querySelector('meta[name="theme-color"]')).toHaveAttribute('content', '#071b24')
  })

  it('restaura a preferência clara antes de renderizar a aplicação', () => {
    localStorage.setItem('rivulus.theme', 'light')
    loadTheme()
    render(<ThemeToggle />)
    expect(document.documentElement).toHaveAttribute('data-theme', 'light')
    expect(screen.getByRole('button', { name: 'Ativar modo noturno' })).toBeInTheDocument()
  })

  it('alterna pelo teclado e mantém a escolha após uma nova inicialização', async () => {
    const user = userEvent.setup()
    loadTheme()
    const { unmount } = render(<ThemeToggle />)
    await user.tab()
    await user.keyboard('{Enter}')
    expect(localStorage.getItem('rivulus.theme')).toBe('light')
    expect(document.querySelector('meta[name="theme-color"]')).toHaveAttribute('content', '#f3f8f8')
    unmount()
    delete document.documentElement.dataset.theme
    loadTheme()
    render(<ThemeToggle />)
    await user.click(screen.getByRole('button', { name: 'Ativar modo noturno' }))
    expect(document.documentElement).toHaveAttribute('data-theme', 'dark')
    expect(localStorage.getItem('rivulus.theme')).toBe('dark')
  })

  it('usa o padrão noturno quando a preferência salva é inválida', () => {
    localStorage.setItem('rivulus.theme', 'invalid')
    loadTheme()
    expect(document.documentElement).toHaveAttribute('data-theme', 'dark')
  })

  it('continua permitindo a troca quando o armazenamento está bloqueado', async () => {
    const user = userEvent.setup()
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => { throw new Error('Blocked') })
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => { throw new Error('Blocked') })
    expect(loadTheme).not.toThrow()
    render(<ThemeToggle />)
    await user.click(screen.getByRole('button', { name: 'Ativar modo claro' }))
    expect(document.documentElement).toHaveAttribute('data-theme', 'light')
  })

  it('sincroniza a preferência entre abas e volta ao padrão se ela for removida', () => {
    loadTheme()
    render(<ThemeToggle />)
    act(() => window.dispatchEvent(new StorageEvent('storage', { key: 'rivulus.theme', newValue: 'light' })))
    expect(screen.getByRole('button', { name: 'Ativar modo noturno' })).toBeInTheDocument()
    act(() => window.dispatchEvent(new StorageEvent('storage', { key: 'rivulus.theme', newValue: null })))
    expect(screen.getByRole('button', { name: 'Ativar modo claro' })).toBeInTheDocument()
  })
})
