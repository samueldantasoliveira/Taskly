import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { MemoryRouter, Route, Routes } from 'react-router'
import { describe, expect, it } from 'vitest'
import { AuthProvider } from '../features/auth/AuthProvider'
import { server } from '../test/setup'
import { LoginPage } from './LoginPage'

describe('LoginPage', () => {
  it('autentica, salva a sessão e segue para a área privada', async () => {
    const user = userEvent.setup()
    server.use(http.post('http://localhost:5219/api/login', async ({ request }) => {
      expect(await request.json()).toEqual({
        email: 'ada@taskly.dev',
        password: 'segredo123',
      })

      return HttpResponse.json({
        token: 'jwt-valido',
        expiresAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
        user: { id: 'user-1', name: 'Ada', email: 'ada@taskly.dev' },
      })
    }))

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={['/login']}>
          <AuthProvider>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/teams" element={<h1>Minhas equipes</h1>} />
            </Routes>
          </AuthProvider>
        </MemoryRouter>
      </QueryClientProvider>,
    )

    await user.type(screen.getByLabelText('E-mail'), 'ada@taskly.dev')
    await user.type(screen.getByLabelText('Senha'), 'segredo123')
    await user.click(screen.getByRole('button', { name: 'Entrar' }))

    expect(await screen.findByRole('heading', { name: 'Minhas equipes' })).toBeInTheDocument()
    expect(JSON.parse(sessionStorage.getItem('taskly.session') ?? '{}')).toMatchObject({
      token: 'jwt-valido',
      user: { email: 'ada@taskly.dev' },
    })
  })
})
