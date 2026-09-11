import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { MemoryRouter, Route, Routes } from 'react-router'
import { describe, expect, it, vi } from 'vitest'
import { AuthContext } from '../features/auth/auth-context'
import { ToastContext } from '../shared/components/toast-context'
import { queryKeys } from '../shared/lib/query-keys'
import type { TodoTask } from '../shared/types/api'
import { server } from '../test/setup'
import { ProjectPage } from './ProjectPage'

const base = 'http://localhost:5219/api'

function setup(count = 21) {
  const state = {
    tasks: Array.from({ length: count }, (_, index): TodoTask => ({
      id: `task-${index}`, title: `Tarefa ${index}`, description: null,
      status: 0, projectId: 'project-1', assignedUserId: null,
      createdAt: '2026-01-01T00:00:00Z', updatedAt: '2026-01-01T00:00:00Z',
    })),
    failSecondPage: false,
  }
  const requests: number[] = []
  server.use(
    http.get(`${base}/project/:id`, ({ params }) => HttpResponse.json({
      id: params.id, name: 'Projeto teste', description: 'Descrição', status: 0, ownerId: 'owner', teamId: 'team-1',
    })),
    http.get(`${base}/team/team-1`, () => HttpResponse.json({ id: 'team-1', ownerId: 'owner', isActive: true })),
    http.get(`${base}/team/team-1/members`, () => HttpResponse.json([])),
    http.get(`${base}/todotask/project/:id`, ({ request }) => {
      const query = new URL(request.url).searchParams
      expect(query.get('pageSize')).toBe('20')
      const page = Number(query.get('page'))
      requests.push(page)
      if (page === 2 && state.failSecondPage) return HttpResponse.text('Falha ao carregar', { status: 500 })
      return HttpResponse.json({ items: state.tasks.slice((page - 1) * 20, page * 20), totalCount: state.tasks.length })
    }),
  )
  const client = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <AuthContext.Provider value={{ user: { id: 'owner', name: 'Owner', email: 'owner@test.dev' }, isAuthenticated: true, isBootstrapping: false, signIn: vi.fn(), signOut: vi.fn(), updateUser: vi.fn() }}>
        <ToastContext.Provider value={{ showToast: vi.fn() }}>
          <MemoryRouter initialEntries={['/projects/project-1']}>
            <Routes><Route path="/projects/:projectId" element={<ProjectPage />} /></Routes>
          </MemoryRouter>
        </ToastContext.Provider>
      </AuthContext.Provider>
    </QueryClientProvider>,
  )
  return { state, requests, client, user: userEvent.setup() }
}

describe('ProjectPage pagination', () => {
  it('lê items e totalCount e navega entre páginas sem misturar os resultados', async () => {
    const { user, requests } = setup()
    expect(await screen.findByText('20 nesta página · 21 no projeto')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Anterior' })).toBeDisabled()
    expect(screen.queryByRole('heading', { name: 'Tarefa 20' })).not.toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Próxima' }))
    expect(await screen.findByRole('heading', { name: 'Tarefa 20' })).toBeInTheDocument()
    expect(screen.queryByRole('heading', { name: 'Tarefa 0' })).not.toBeInTheDocument()
    expect(screen.getByText('Página 2 de 2')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Próxima' })).toBeDisabled()
    await user.click(screen.getByRole('button', { name: 'Anterior' }))
    expect(await screen.findByRole('heading', { name: 'Tarefa 0' })).toBeInTheDocument()
    expect(requests).toContain(1)
    expect(requests).toContain(2)
  })

  it('explica o escopo da busca e diferencia busca vazia de projeto vazio', async () => {
    const { user } = setup()
    await screen.findByRole('heading', { name: 'Tarefa 0' })
    await user.type(screen.getByRole('textbox', { name: 'Buscar tarefas nesta página' }), 'Tarefa 20')
    expect(screen.getByText('Nenhuma tarefa encontrada nesta página')).toBeInTheDocument()
    expect(screen.queryByText('O quadro está vazio')).not.toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Próxima' }))
    expect(await screen.findByRole('heading', { name: 'Tarefa 20' })).toBeInTheDocument()
  })

  it('mostra estado vazio e desabilita navegação para um projeto sem tarefas', async () => {
    setup(0)
    expect(await screen.findByText('O quadro está vazio')).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Anterior' })).toBeDisabled()
    expect(screen.getByRole('button', { name: 'Próxima' })).toBeDisabled()
  })

  it('permite voltar após falha ao carregar outra página', async () => {
    const { user, state } = setup()
    await screen.findByRole('heading', { name: 'Tarefa 0' })
    state.failSecondPage = true
    await user.click(screen.getByRole('button', { name: 'Próxima' }))
    expect(await screen.findByText('Falha ao carregar')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Anterior' }))
    expect(await screen.findByRole('heading', { name: 'Tarefa 0' })).toBeInTheDocument()
  })

  it('volta à última página válida quando uma atualização esvazia a página atual', async () => {
    const { user, state, client } = setup()
    await screen.findByRole('heading', { name: 'Tarefa 0' })
    await user.click(screen.getByRole('button', { name: 'Próxima' }))
    await screen.findByRole('heading', { name: 'Tarefa 20' })
    state.tasks.pop()
    await client.invalidateQueries({ queryKey: queryKeys.tasks('project-1') })
    await waitFor(() => expect(screen.getByText('Página 1 de 1')).toBeInTheDocument())
    expect(await screen.findByText('20 nesta página · 20 no projeto')).toBeInTheDocument()
  })

  it('exclui a última tarefa da segunda página e atualiza o quadro e o total', async () => {
    const { user, state } = setup()
    server.use(http.delete(`${base}/todotask/task-20`, () => {
      state.tasks = state.tasks.filter((task) => task.id !== 'task-20')
      return new HttpResponse(null, { status: 204 })
    }))
    await screen.findByRole('heading', { name: 'Tarefa 0' })
    await user.click(screen.getByRole('button', { name: 'Próxima' }))
    await user.click(await screen.findByRole('button', { name: 'Editar Tarefa 20' }))
    await user.click(screen.getByRole('button', { name: 'Excluir' }))
    await user.click(screen.getByRole('button', { name: 'Confirmar exclusão' }))
    expect(await screen.findByText('20 nesta página · 20 no projeto')).toBeInTheDocument()
    expect(screen.getByText('Página 1 de 1')).toBeInTheDocument()
  })

  it('ao criar uma tarefa volta à primeira página e limpa a busca', async () => {
    const { user, state } = setup()
    server.use(http.post(`${base}/todotask`, async ({ request }) => {
      const input = await request.json() as { title: string; description: string; projectId: string }
      const created = { ...state.tasks[0], ...input, id: 'new-task' }
      state.tasks.unshift(created)
      return HttpResponse.json(created)
    }))
    await screen.findByRole('heading', { name: 'Tarefa 0' })
    await user.click(screen.getByRole('button', { name: 'Próxima' }))
    await screen.findByRole('heading', { name: 'Tarefa 20' })
    await user.type(screen.getByRole('textbox', { name: 'Buscar tarefas nesta página' }), 'Tarefa 20')
    await user.click(screen.getByRole('button', { name: 'Nova tarefa' }))
    await user.type(screen.getByLabelText('Título'), 'Tarefa recém-criada')
    await user.click(screen.getByRole('button', { name: 'Criar tarefa' }))
    expect(await screen.findByRole('heading', { name: 'Tarefa recém-criada' })).toBeInTheDocument()
    expect(screen.getByText('Página 1 de 2')).toBeInTheDocument()
    expect(screen.getByRole('textbox', { name: 'Buscar tarefas nesta página' })).toHaveValue('')
    expect(screen.getByText('20 nesta página · 22 no projeto')).toBeInTheDocument()
  })
})
