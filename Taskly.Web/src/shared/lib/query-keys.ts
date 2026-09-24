import type { Id } from '../types/api'

export const queryKeys = {
  myWork: ['my-work'] as const,
  teams: ['teams'] as const,
  team: (id: Id) => ['teams', id] as const,
  members: (teamId: Id) => ['teams', teamId, 'members'] as const,
  invitations: (teamId: Id) => ['teams', teamId, 'invitations'] as const,
  projects: (teamId: Id) => ['teams', teamId, 'projects'] as const,
  project: (id: Id) => ['projects', id] as const,
  tasks: (projectId: Id) => ['projects', projectId, 'tasks'] as const,
  activities: (projectId: Id) => ['projects', projectId, 'activities'] as const,
  taskList: (projectId: Id, scope: string, filters: object) =>
    ['projects', projectId, 'tasks', scope, filters] as const,
}
