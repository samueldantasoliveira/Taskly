import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { server } from '../../test/setup'
import { AuthProvider } from './AuthProvider'
import { useAuth } from './auth-context'
import { readSession, writeSession } from './auth-storage'

const user = { id: 'user-1', name: 'Ada', email: 'ada@example.test' }
function Status() {
  const { isAuthenticated, isBootstrapping } = useAuth()
  return <p>{isBootstrapping ? 'Loading session' : isAuthenticated ? 'Authenticated' : 'Signed out'}</p>
}
function setup() {
  writeSession({ token: 'saved-token', expiresAt: new Date(Date.now() + 3600000).toISOString(), user })
  render(<QueryClientProvider client={new QueryClient()}><AuthProvider><Status /></AuthProvider></QueryClientProvider>)
  return userEvent.setup()
}

describe('AuthProvider bootstrap', () => {
  it.each(['network', 'server'])('preserva a sessão na falha %s e permite tentar novamente', async failure => {
    server.use(http.get('http://localhost:5219/api/user/me', () => failure === 'network' ? HttpResponse.error() : new HttpResponse(null, { status: 503 })))
    const interactions = setup()
    expect(await screen.findByText(/Ela foi preservada/)).toBeInTheDocument()
    expect(readSession()?.token).toBe('saved-token')
    server.use(http.get('http://localhost:5219/api/user/me', () => HttpResponse.json(user)))
    await interactions.click(screen.getByRole('button', { name: /tentar novamente/i }))
    expect(await screen.findByText('Authenticated')).toBeInTheDocument()
    expect(readSession()?.user).toEqual(user)
  })

  it.each([401, 404])('remove a sessão definitivamente inválida (%s)', async status => {
    server.use(http.get('http://localhost:5219/api/user/me', () => new HttpResponse(null, { status })))
    setup()
    expect(await screen.findByText('Signed out')).toBeInTheDocument()
    await waitFor(() => expect(readSession()).toBeNull())
  })
})
