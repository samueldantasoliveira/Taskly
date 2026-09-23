import { http, HttpResponse } from 'msw'
import { describe, expect, it } from 'vitest'
import { TaskPriority, TodoStatus, type TodoTask } from '../../shared/types/api'
import { server } from '../../test/setup'
import { getAllProjectTasks } from './api'

const base = 'http://localhost:5219/api'

describe('tasks API', () => {
  it('busca todas as páginas de uma coleção ativa', async () => {
    const tasks = Array.from({ length: 101 }, (_, index): TodoTask => ({
      id: `task-${index}`,
      title: `Tarefa ${index}`,
      description: null,
      status: TodoStatus.Todo,
      priority: TaskPriority.Medium,
      dueDate: null,
      projectId: 'project-1',
      assignedUserId: null,
      createdAt: '2026-01-01T00:00:00Z',
      updatedAt: '2026-01-01T00:00:00Z',
    }))
    const requestedPages: number[] = []
    server.use(http.get(`${base}/todotask/project/project-1`, ({ request }) => {
      const query = new URL(request.url).searchParams
      const page = Number(query.get('page'))
      const pageSize = Number(query.get('pageSize'))
      requestedPages.push(page)
      expect(query.get('status')).toBe(String(TodoStatus.Todo))
      expect(pageSize).toBe(100)
      return HttpResponse.json({
        items: tasks.slice((page - 1) * pageSize, page * pageSize),
        totalCount: tasks.length,
      })
    }))

    const result = await getAllProjectTasks('project-1', {
      status: TodoStatus.Todo,
      sortBy: 'CreatedAt',
      sortDirection: 'Descending',
    })

    expect(requestedPages).toEqual([1, 2])
    expect(result.items).toHaveLength(101)
    expect(result.totalCount).toBe(101)
  })
})
