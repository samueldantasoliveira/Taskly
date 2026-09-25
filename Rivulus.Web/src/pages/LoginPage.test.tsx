import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { MemoryRouter, Route, Routes } from 'react-router'
import { describe, expect, it } from 'vitest'
import { AuthProvider } from '../features/auth/AuthProvider'
import { server } from '../test/setup'
import { LoginPage } from './LoginPage'
import { RegisterPage } from './RegisterPage'

describe('LoginPage', () => {
  it('autentica, salva a sessão e segue para a área privada', async () => {
    const user = userEvent.setup()
    server.use(http.post('http://localhost:5219/api/login', async ({ request }) => {
      expect(await request.json()).toEqual({
        email: 'ada@rivulus.dev',
        password: 'segredo123',
      })

      return HttpResponse.json({
        token: 'jwt-valido',
        expiresAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
        user: { id: 'user-1', name: 'Ada', email: 'ada@rivulus.dev' },
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

    await user.type(screen.getByLabelText('E-mail'), 'ada@rivulus.dev')
    await user.type(screen.getByLabelText('Senha'), 'segredo123')
    await user.click(screen.getByRole('button', { name: 'Entrar' }))

    expect(await screen.findByRole('heading', { name: 'Minhas equipes' })).toBeInTheDocument()
    expect(JSON.parse(sessionStorage.getItem('rivulus.session') ?? '{}')).toMatchObject({
      token: 'jwt-valido',
      user: { email: 'ada@rivulus.dev' },
    })
  })

  it('preserva o convite ao seguir do login para o cadastro', async () => {
    const user = userEvent.setup()
    const invitedUser = { id: 'user-2', name: 'Grace', email: 'grace@rivulus.dev' }

    server.use(
      http.post('http://localhost:5219/api/user', async ({ request }) => {
        expect(await request.json()).toEqual({
          name: 'Grace',
          email: 'grace@rivulus.dev',
          password: 'segredo123',
        })
        return HttpResponse.json(invitedUser, { status: 201 })
      }),
      http.post('http://localhost:5219/api/login', () => HttpResponse.json({
        token: 'jwt-convidado',
        expiresAt: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
        user: invitedUser,
      })),
    )

    const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
    render(
      <QueryClientProvider client={queryClient}>
        <MemoryRouter initialEntries={[{ pathname: '/login', state: { from: '/invitations/token-do-convite' } }]}>
          <AuthProvider>
            <Routes>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
              <Route path="/invitations/:token" element={<h1>Convite preservado</h1>} />
            </Routes>
          </AuthProvider>
        </MemoryRouter>
      </QueryClientProvider>,
    )

    await user.click(screen.getByRole('link', { name: 'Criar conta' }))
    await user.type(screen.getByLabelText('Nome'), 'Grace')
    await user.type(screen.getByLabelText('E-mail'), 'grace@rivulus.dev')
    await user.type(screen.getByLabelText('Senha'), 'segredo123')
    await user.click(screen.getByRole('button', { name: 'Criar minha conta' }))

    expect(await screen.findByRole('heading', { name: 'Convite preservado' })).toBeInTheDocument()
  })
})
