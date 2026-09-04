import { apiRequest, jsonBody } from '../../shared/api/client'
import type {
  CreateProjectInput,
  Id,
  Project,
  UpdateProjectInput,
} from '../../shared/types/api'

export function getTeamProjects(teamId: Id, signal?: AbortSignal) {
  return apiRequest<Project[]>(`/api/project/team/${teamId}`, { signal })
}

export function getProject(id: Id, signal?: AbortSignal) {
  return apiRequest<Project>(`/api/project/${id}`, { signal })
}

export function createProject(input: CreateProjectInput) {
  return apiRequest<Project>('/api/project', {
    method: 'POST',
    body: jsonBody(input),
  })
}

export function updateProject(id: Id, input: UpdateProjectInput) {
  return apiRequest<Project>(`/api/project/${id}`, {
    method: 'PUT',
    body: jsonBody(input),
  })
}

export function deleteProject(id: Id) {
  return apiRequest<void>(`/api/project/${id}`, { method: 'DELETE' })
}
