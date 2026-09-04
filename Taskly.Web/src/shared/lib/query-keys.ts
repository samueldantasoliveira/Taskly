import type { Id } from '../types/api'

export const queryKeys = {
  teams: ['teams'] as const,
  team: (id: Id) => ['teams', id] as const,
  members: (teamId: Id) => ['teams', teamId, 'members'] as const,
  projects: (teamId: Id) => ['teams', teamId, 'projects'] as const,
  project: (id: Id) => ['projects', id] as const,
  tasks: (projectId: Id) => ['projects', projectId, 'tasks'] as const,
}
