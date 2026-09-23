import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { http, HttpResponse } from 'msw'
import { MemoryRouter, Route, Routes } from 'react-router'
import { describe, expect, it, vi } from 'vitest'
import { AuthContext } from '../features/auth/auth-context'
import { ToastContext } from '../shared/components/toast-context'
import { TaskPriority, TodoStatus, type TodoTask } from '../shared/types/api'
import { server } from '../test/setup'
import { ProjectPage } from './ProjectPage'

const base = 'http://localhost:5219/api'

function task(index: number, status: TodoTask['status'], overrides: Partial<TodoTask> = {}): TodoTask {
  return {
    id: `task-${status}-${index}`,
    title: `Tarefa ${status}-${index}`,
    description: null,
    status,
    priority: TaskPriority.Medium,
    dueDate: null,
    projectId: 'project-1',
    assignedUserId: null,
    createdAt: new Date(Date.UTC(2026, 0, index + 1)).toISOString(),
    updatedAt: '2026-01-01T00:00:00Z',
    ...overrides,
  }
}

function setup(initialTasks: TodoTask[]) {
  const state = { tasks: initialTasks }
  const requests: URLSearchParams[] = []
  server.use(
    http.get(`${base}/project/:id`, ({ params }) => HttpResponse.json({
      id: params.id,
      name: 'Projeto teste',
      description: 'Descrição',
      status: 0,
      ownerId: 'owner',
      teamId: 'team-1',
    })),
    http.get(`${base}/team/team-1`, () => HttpResponse.json({
      id: 'team-1',
      ownerId: 'owner',
      isActive: true,
    })),
    http.get(`${base}/team/team-1/members`, () => HttpResponse.json([
      { id: 'owner', name: 'Owner', email: 'owner@test.dev', isOwner: true },
      { id: 'member-1', name: 'Member', email: 'member@test.dev', isOwner: false },
    ])),
    http.get(`${base}/todotask/project/:id`, ({ request }) => {
      const query = new URL(request.url).searchParams
      requests.push(new URLSearchParams(query))
      const status = Number(query.get('status'))
      const title = query.get('title')?.toLowerCase()
      const assigneeId = query.get('assigneeId')
      const sortBy = query.get('sortBy') ?? 'CreatedAt'
      const direction = query.get('sortDirection') === 'Ascending' ? 1 : -1
      const page = Number(query.get('page'))
      const pageSize = Number(query.get('pageSize'))
      const filtered = state.tasks
        .filter((item) => item.status === status)
        .filter((item) => !title || item.title.toLowerCase().includes(title))
        .filter((item) => !assigneeId || item.assignedUserId === assigneeId)
        .sort((left, right) => {
          const leftValue = sortBy === 'Title' ? left.title : sortBy === 'Status' ? left.status : left.createdAt
          const rightValue = sortBy === 'Title' ? right.title : sortBy === 'Status' ? right.status : right.createdAt
          const comparison = String(leftValue).localeCompare(String(rightValue))
          return comparison === 0 ? left.id.localeCompare(right.id) : comparison * direction
        })
      return HttpResponse.json({
        items: filtered.slice((page - 1) * pageSize, page * pageSize),
        totalCount: filtered.length,
      })
    }),
  )
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  })
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

describe('ProjectPage task queries', () => {
  it('informa salvamento parcial e usa a versão salva ao tentar novamente', async () => {
    const item = task(0, TodoStatus.Todo, { title: 'Original', version: 1 })
    const { user, state } = setup([item])
    const versions: unknown[] = []
    let assignmentFails = true
    server.use(
      http.put(`${base}/todotask/:id`, async ({ request }) => {
        const body = await request.json() as { title: string, description: string, version: number }
        versions.push(body.version)
        state.tasks[0] = { ...state.tasks[0], ...body, version: body.version + 1 }
        return HttpResponse.json(state.tasks[0])
      }),
      http.post(`${base}/todotask/:id/assign`, () => {
        if (assignmentFails) return HttpResponse.json({ message: 'Indisponível' }, { status: 503 })
        state.tasks[0] = { ...state.tasks[0], assignedUserId: 'member-1' }
        return HttpResponse.json(state.tasks[0])
      }),
    )
    await user.click(await screen.findByRole('button', { name: 'Editar Original' }))
    const dialog = within(screen.getByRole('dialog'))
    await user.clear(dialog.getByLabelText('Título'))
    await user.type(dialog.getByLabelText('Título'), 'Título salvo')
    await user.selectOptions(dialog.getByLabelText('Responsável'), 'member-1')
    await user.click(dialog.getByRole('button', { name: 'Salvar' }))
    expect(await dialog.findByText(/Título e descrição foram salvos/)).toBeInTheDocument()
    expect(await screen.findByRole('heading', { name: 'Título salvo' })).toBeInTheDocument()
    assignmentFails = false
    await waitFor(() => expect(dialog.getByRole('button', { name: 'Salvar' })).not.toBeDisabled())
    await user.click(dialog.getByRole('button', { name: 'Salvar' }))
    await waitFor(() => expect(screen.queryByRole('dialog')).not.toBeInTheDocument())
    expect(versions).toEqual([1, 2])
    expect(state.tasks[0].assignedUserId).toBe('member-1')
  })

  it('mostra as tarefas ativas e pagina os históricos de forma independente', async () => {
    const tasks = [
      ...Array.from({ length: 2 }, (_, index) => task(index, TodoStatus.Todo)),
      ...Array.from({ length: 21 }, (_, index) => task(index, TodoStatus.Done)),
      ...Array.from({ length: 21 }, (_, index) => task(index, TodoStatus.Cancelled)),
    ]
    const { requests, user } = setup(tasks)

    expect(await screen.findByRole('heading', { name: 'Tarefa 0-1' })).toBeInTheDocument()
    expect(screen.queryByRole('heading', { name: 'Tarefa 2-0' })).not.toBeInTheDocument()
    expect(screen.queryByRole('heading', { name: 'Tarefa 3-0' })).not.toBeInTheDocument()
    expect(requests.some((query) => query.get('status') === '0' && query.get('pageSize') === '100')).toBe(true)

    await user.click(screen.getByRole('button', { name: 'Carregar mais concluídas' }))
    expect(await screen.findByRole('heading', { name: 'Tarefa 2-0' })).toBeInTheDocument()
    expect(screen.queryByRole('heading', { name: 'Tarefa 3-0' })).not.toBeInTheDocument()
    expect(requests.some((query) => query.get('status') === '2' && query.get('page') === '2')).toBe(true)
    expect(requests.some((query) => query.get('status') === '3' && query.get('page') === '2')).toBe(false)
  })

  it('envia busca e responsável ao backend e combina os filtros', async () => {
    const tasks = [
      task(0, TodoStatus.Todo, { title: 'Alpha API', assignedUserId: 'owner' }),
      task(1, TodoStatus.InProgress, { title: 'Bravo API', assignedUserId: 'member-1' }),
      task(2, TodoStatus.Done, { title: 'Charlie Web', assignedUserId: 'member-1' }),
      task(3, TodoStatus.Done, { title: 'Delta API', assignedUserId: 'member-1' }),
    ]
    const { requests, user } = setup(tasks)
    await screen.findByRole('heading', { name: 'Alpha API' })

    await user.type(screen.getByRole('textbox', { name: 'Buscar tarefas por título' }), 'api')
    await user.selectOptions(screen.getByRole('combobox', { name: 'Filtrar por responsável' }), 'member-1')

    expect(await screen.findByText('2 tarefas encontradas')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Bravo API' })).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Delta API' })).toBeInTheDocument()
    expect(screen.queryByRole('heading', { name: 'Alpha API' })).not.toBeInTheDocument()
    await waitFor(() => expect(requests.filter((query) =>
      query.get('title') === 'api' && query.get('assigneeId') === 'member-1',
    )).toHaveLength(4))
  })

  it('ordena cada coluna usando a opção selecionada', async () => {
    const tasks = [
      task(0, TodoStatus.Todo, { title: 'Charlie' }),
      task(1, TodoStatus.Todo, { title: 'Alpha' }),
      task(2, TodoStatus.Todo, { title: 'Bravo' }),
    ]
    const { requests, user } = setup(tasks)
    await screen.findByRole('heading', { name: 'Charlie' })

    await user.selectOptions(screen.getByRole('combobox', { name: 'Ordenar tarefas' }), 'titleAscending')

    const todoColumn = screen.getByRole('region', { name: 'A fazer' })
    await waitFor(() => expect(
      within(todoColumn).getAllByRole('heading', { level: 3 }).map((heading) => heading.textContent),
    ).toEqual(['Alpha', 'Bravo', 'Charlie']))
    expect(requests.some((query) =>
      query.get('sortBy') === 'Title' && query.get('sortDirection') === 'Ascending',
    )).toBe(true)
  })

  it('limpa os filtros depois de criar uma tarefa para exibi-la no quadro', async () => {
    const { state, user } = setup([
      task(0, TodoStatus.Todo, { title: 'Tarefa existente' }),
    ])
    server.use(http.post(`${base}/todotask`, async ({ request }) => {
      const input = await request.json() as { title: string; description: string; projectId: string; assignedUserId: string | null }
      const created = task(1, TodoStatus.Todo, { ...input, id: 'new-task' })
      state.tasks.push(created)
      return HttpResponse.json(created)
    }))
    await screen.findByRole('heading', { name: 'Tarefa existente' })
    await user.type(screen.getByRole('textbox', { name: 'Buscar tarefas por título' }), 'não encontrada')
    expect(await screen.findByText('Nenhuma tarefa encontrada')).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Nova tarefa' }))
    await user.type(screen.getByLabelText('Título'), 'Tarefa recém-criada')
    await user.click(screen.getByRole('button', { name: 'Criar tarefa' }))

    expect(await screen.findByRole('heading', { name: 'Tarefa recém-criada' })).toBeInTheDocument()
    expect(screen.getByRole('textbox', { name: 'Buscar tarefas por título' })).toHaveValue('')
  })
})
