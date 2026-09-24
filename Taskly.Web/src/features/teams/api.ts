import { apiRequest, jsonBody } from '../../shared/api/client'
import type {
  CreateTeamInput,
  Id,
  Team,
  TeamMember,
  TeamInvitation,
  UpdateTeamInput,
} from '../../shared/types/api'

export function getTeams(signal?: AbortSignal) {
  return apiRequest<Team[]>('/api/team', { signal })
}

export function getTeam(id: Id, signal?: AbortSignal) {
  return apiRequest<Team>(`/api/team/${id}`, { signal })
}

export function getTeamMembers(id: Id, signal?: AbortSignal) {
  return apiRequest<TeamMember[]>(`/api/team/${id}/members`, { signal })
}

export function createTeam(input: CreateTeamInput) {
  return apiRequest<Team>('/api/team', {
    method: 'POST',
    body: jsonBody(input),
  })
}

export function updateTeam(id: Id, input: UpdateTeamInput) {
  return apiRequest<Team>(`/api/team/${id}`, {
    method: 'PUT',
    body: jsonBody(input),
  })
}

export function addTeamMember(teamId: Id, userId: Id) {
  const query = new URLSearchParams({ userId })
  return apiRequest<void>(`/api/team/${teamId}/add-member?${query}`, {
    method: 'POST',
  })
}

export function removeTeamMember(teamId: Id, userId: Id) {
  const query = new URLSearchParams({ userId })
  return apiRequest<void>(`/api/team/${teamId}/remove-member?${query}`, {
    method: 'DELETE',
  })
}

export function deleteTeam(id: Id) {
  return apiRequest<void>(`/api/team/${id}`, { method: 'DELETE' })
}

export function leaveTeam(id: Id) {
  return apiRequest<void>(`/api/team/${id}/leave`, { method: 'DELETE' })
}

export function getTeamInvitations(teamId: Id, signal?: AbortSignal) { return apiRequest<TeamInvitation[]>(`/api/team/${teamId}/invitations`, { signal }) }
export function createTeamInvitation(teamId: Id, email: string) { return apiRequest<TeamInvitation>(`/api/team/${teamId}/invitations`, { method: 'POST', body: jsonBody({ email }) }) }
export function revokeTeamInvitation(teamId: Id, invitationId: Id) { return apiRequest<void>(`/api/team/${teamId}/invitations/${invitationId}`, { method: 'DELETE' }) }
export function acceptTeamInvitation(token: string) { return apiRequest<Team>(`/api/team/invitations/${encodeURIComponent(token)}/accept`, { method: 'POST' }) }
