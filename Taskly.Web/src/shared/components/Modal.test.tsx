import { useState } from 'react'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import { Modal } from './Modal'

function Example() {
  const [open, setOpen] = useState(false)
  return <><button onClick={() => setOpen(true)}>Abrir</button><Modal open={open} title="Editar" onClose={() => setOpen(false)}>
    <input aria-label="Nome" autoFocus /><button disabled>Indisponível</button><button>Salvar</button>
  </Modal></>
}

describe('Modal keyboard navigation', () => {
  it('mantém Tab e Shift+Tab no diálogo e devolve foco após Escape', async () => {
    const user = userEvent.setup()
    render(<Example />)
    const opener = screen.getByRole('button', { name: 'Abrir' })
    await user.click(opener)
    expect(screen.getByRole('dialog', { name: 'Editar' })).toBeInTheDocument()
    expect(screen.getByRole('textbox', { name: 'Nome' })).toHaveFocus()
    await user.tab()
    expect(screen.getByRole('button', { name: 'Salvar' })).toHaveFocus()
    await user.tab()
    expect(screen.getByRole('button', { name: 'Fechar' })).toHaveFocus()
    await user.tab({ shift: true })
    expect(screen.getByRole('button', { name: 'Salvar' })).toHaveFocus()
    await user.keyboard('{Escape}')
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
    expect(opener).toHaveFocus()
  })
})
