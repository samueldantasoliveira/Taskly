import { apiRequest, jsonBody } from '../../shared/api/client'
import type {
  CreateTaskInput,
  Id,
  PagedResult,
  TodoTask,
  TodoStatus,
  UpdateTaskInput,
} from '../../shared/types/api'

export type TodoTaskSortBy = 'CreatedAt' | 'Title' | 'Status'
export type TodoTaskSortDirection = 'Ascending' | 'Descending'

export interface ProjectTaskQuery {
  page?: number
  pageSize?: number
  title?: string
  status?: TodoStatus
  assigneeId?: Id
  sortBy?: TodoTaskSortBy
  sortDirection?: TodoTaskSortDirection
}

export function getProjectTasks(projectId: Id, options: ProjectTaskQuery = {}, signal?: AbortSignal) {
  const query = new URLSearchParams({
    page: String(options.page ?? 1),
    pageSize: String(options.pageSize ?? 20),
  })
  if (options.title) query.set('title', options.title)
  if (options.status !== undefined) query.set('status', String(options.status))
  if (options.assigneeId) query.set('assigneeId', options.assigneeId)
  if (options.sortBy) query.set('sortBy', options.sortBy)
  if (options.sortDirection) query.set('sortDirection', options.sortDirection)
  return apiRequest<PagedResult<TodoTask>>(`/api/todotask/project/${projectId}?${query}`, { signal })
}

export async function getAllProjectTasks(
  projectId: Id,
  options: Omit<ProjectTaskQuery, 'page' | 'pageSize'>,
  signal?: AbortSignal,
) {
  const pageSize = 100
  const items: TodoTask[] = []
  let page = 1
  let totalCount = 0

  do {
    const result = await getProjectTasks(projectId, { ...options, page, pageSize }, signal)
    totalCount = result.totalCount
    items.push(...result.items)
    if (result.items.length === 0) break
    page += 1
  } while (items.length < totalCount)

  return { items, totalCount }
}

export function createTask(input: CreateTaskInput) {
  return apiRequest<TodoTask>('/api/todotask', {
    method: 'POST',
    body: jsonBody(input),
  })
}

export function updateTask(id: Id, input: UpdateTaskInput) {
  return apiRequest<TodoTask>(`/api/todotask/${id}`, {
    method: 'PUT',
    body: jsonBody(input),
  })
}

export function deleteTask(id: Id) {
  return apiRequest<void>(`/api/todotask/${id}`, { method: 'DELETE' })
}

export function assignTask(id: Id, userId: Id | null) {
  return apiRequest<void>(`/api/todotask/${id}/assign`, {
    method: 'POST',
    body: jsonBody({ userId }),
  })
}

export function startTask(id: Id) {
  return apiRequest<void>(`/api/todotask/${id}/start`, { method: 'POST' })
}

export function completeTask(id: Id) {
  return apiRequest<void>(`/api/todotask/${id}/complete`, { method: 'POST' })
}

export function cancelTask(id: Id) {
  return apiRequest<void>(`/api/todotask/${id}/cancel`, { method: 'POST' })
}
