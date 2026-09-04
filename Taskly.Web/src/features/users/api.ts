import { apiRequest, jsonBody } from '../../shared/api/client'
import type { Id, UpdateUserInput, User } from '../../shared/types/api'

export function searchUser(email: string, signal?: AbortSignal) {
  const query = new URLSearchParams({ email })
  return apiRequest<User>(`/api/user/search?${query}`, { signal })
}

export function updateUser(id: Id, input: UpdateUserInput) {
  return apiRequest<User>(`/api/user/${id}`, {
    method: 'PUT',
    body: jsonBody(input),
  })
}

export function deleteUser(id: Id) {
  return apiRequest<void>(`/api/user/${id}`, { method: 'DELETE' })
}
