import { http, HttpResponse } from 'msw'
import { describe, expect, it, vi } from 'vitest'
import { writeSession } from '../../features/auth/auth-storage'
import { server } from '../../test/setup'
import { ApiError, apiRequest, jsonBody } from './client'

const apiUrl = 'http://localhost:5219'

describe('api client', () => {
  it('envia JSON e o token da sessão', async () => {
    writeSession({
      token: 'jwt-teste',
      expiresAt: '2099-01-01T00:00:00.000Z',
      user: { id: '1', name: 'Ada', email: 'ada@taskly.dev' },
    })

    server.use(http.post(`${apiUrl}/api/example`, async ({ request }) => {
      expect(request.headers.get('authorization')).toBe('Bearer jwt-teste')
      expect(request.headers.get('content-type')).toBe('application/json')
      expect(await request.json()).toEqual({ title: 'Nova tarefa' })
      return HttpResponse.json({ id: 'task-1' })
    }))

    await expect(apiRequest('/api/example', {
      method: 'POST',
      body: jsonBody({ title: 'Nova tarefa' }),
    })).resolves.toEqual({ id: 'task-1' })
  })

  it('normaliza os erros de validação da API', async () => {
    server.use(http.post(`${apiUrl}/api/example`, () => HttpResponse.json({
      errors: { Name: ['Nome é obrigatório.'], Email: ['E-mail inválido.'] },
    }, { status: 400 })))

    const request = apiRequest('/api/example', { method: 'POST' })

    await expect(request).rejects.toMatchObject<ApiError>({
      name: 'ApiError',
      status: 400,
      message: 'Nome é obrigatório. E-mail inválido.',
    })
  })

  it('limpa a sessão e avisa a aplicação após um 401 autenticado', async () => {
    writeSession({
      token: 'jwt-expirado',
      expiresAt: '2099-01-01T00:00:00.000Z',
      user: { id: '1', name: 'Ada', email: 'ada@taskly.dev' },
    })
    const listener = vi.fn()
    window.addEventListener('taskly:unauthorized', listener)
    server.use(http.get(`${apiUrl}/api/protected`, () => new HttpResponse(null, { status: 401 })))

    await expect(apiRequest('/api/protected')).rejects.toBeInstanceOf(ApiError)
    expect(sessionStorage.getItem('taskly.session')).toBeNull()
    expect(listener).toHaveBeenCalledOnce()
    window.removeEventListener('taskly:unauthorized', listener)
  })
})
