import { apiRequest, jsonBody } from '../../shared/api/client'
import type {
  CreateTaskInput,
  Id,
  TodoTask,
  UpdateTaskInput,
} from '../../shared/types/api'

export function getProjectTasks(projectId: Id, signal?: AbortSignal) {
  return apiRequest<TodoTask[]>(`/api/todotask/project/${projectId}`, { signal })
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
